using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using DomainGateway.Components;
using DomainGateway.ConfigurationProviders;
using DomainGateway.ConfigurationProviders.AwsS3;
using DomainGateway.ConfigurationProviders.AzBlobStorage;
using DomainGateway.ConfigurationProviders.FileSystem;
using DomainGateway.Configurations;
using DomainGateway.Contracts;
using DomainGateway.Database;
using DomainGateway.Infrastructure;
using DomainGateway.RateLimiting;
using DomainGateway.ServiceDiscovery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using SoftEEring.Core.Helpers;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// remove logger providers and register OTel only
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(options =>
{
	options.SetResourceBuilder(ResourceBuilder.CreateEmpty()
		.AddService("DomainGateway")
		.AddAttributes(new Dictionary<string, object>
		{
			["environment"] = builder.Environment.EnvironmentName
		}));

	options.AddConsoleExporter();

	// USE SEQ for OTEL
	// options.AddOtlpExporter(otlpOptions =>
	// {
	// 	otlpOptions.Endpoint = new Uri("https://localhost:8500");
	// 	otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
	// 	otlpOptions.Headers = "api-key=your_api_key";
	// });
});

builder.AddServiceDefaults();

builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
var configurationSection = builder.Configuration.GetSection("DomainGatewayConfiguration");
builder.Services.Configure<DomainGatewaySetup>(configurationSection);
var configuration = configurationSection.Get<DomainGatewaySetup>()!;

builder.Services.AddDbContext<DomainGatewayDbContext>();

// Add rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.AddPolicy(RateLimiterConfiguration.SlidingWindowRateLimiterPolicyName, context =>
	{
		// Extract clientId from header, fallback unknown if not present
		var clientId = context.Request.Headers[RateLimiterConfiguration.ClientIdHeaderName].FirstOrDefault() ??
		               RateLimiterConfiguration.FallbackClientIdHeaderValue;
		var configProvider = context.RequestServices.GetRequiredService<IGatewayConfigurationService>();
		var (defaultConfig, pathSpecificConfig) = configProvider.GetRateLimiterConfiguration().GetApplicableConfig(clientId, context.Request.Path);

		if (pathSpecificConfig?.Disabled == true || defaultConfig.Disabled)
			return RateLimitPartition.GetNoLimiter(clientId);

		var partitionKey = pathSpecificConfig is null ? clientId : $"{clientId}__{context.Request.Path}";
		var applicableConfig = pathSpecificConfig ?? defaultConfig;

		return RateLimitPartition.GetSlidingWindowLimiter(
			partitionKey: partitionKey,
			factory: _ => new SlidingWindowRateLimiterOptions
			{
				PermitLimit = applicableConfig.PermitLimit,
				Window = applicableConfig.PermitLimitWindow,
				SegmentsPerWindow = applicableConfig.SegmentsPerWindow,
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
				QueueLimit = applicableConfig.QueueLimit
			}
		);
	});

	options.OnRejected = (context, _) =>
	{
		// TODO add open telemetry event
		// var clientId = context.HttpContext.Request.Headers[RateLimiter.ClientIdHeaderName].FirstOrDefault() ?? "unknown";
		context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		return ValueTask.CompletedTask;
	};
});

// Example: log the S3 bucket if present
if (configuration.FileSystemRepository is not null)
{
	// TODO add some logging
	builder.Services.AddFileSystemProvider(configuration.FileSystemRepository);
}
else if (configuration.AwsS3Repository is not null)
{
	// TODO add some logging
	builder.Services.AddAwsS3Provider(configuration.AwsS3Repository);
}
else if (configuration.AzBlobStorageRepository is not null)
{
	// TODO add some logging
	builder.Services.AddAzBlobStorageProvider(configuration.AzBlobStorageRepository);
}
else
{
	throw new Exception("No Gateway configuration provider found in setup.");
}

builder.Services.AddReverseProxy()
	.ConfigureHttpClient((context, handler) =>
	{
		// handler.MaxConnectionsPerServer = 100;
		// handler.EnableMultipleHttp2Connections = true;
	})
	.AddTransforms(builderContext =>
	{
		builderContext.AddRequestTransform(transformContext =>
		{
			var request = transformContext.HttpContext.Request;
			Console.WriteLine($"YARP Request handling: {request.HttpContext} mapped to {transformContext.DestinationPrefix}");
			return ValueTask.CompletedTask;
		});
	}); //.LoadFromMemory([], []);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddScoped<ClientIdHeaderFilter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IServiceDiscoveryInstancesRepository, InstancesDatabaseRepository>();
builder.Services.AddSingleton<GatewayConfigurationService>();
builder.Services.AddSingleton<IGatewayConfigurationService>(sp => sp.GetRequiredService<GatewayConfigurationService>());
builder.Services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<GatewayConfigurationService>());
builder.Services.AddHostedService<InstanceCleanUpJob>();
builder.Services.AddHostedService<GatewayConfigurationSyncJob>();

if (configuration.EnableAdminPortal)
{
	builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
}

var app = builder.Build();

// migrate database on startup
app.Services.GetScopedService(out DomainGatewayDbContext dbContext).Using(_ =>
{
	if (dbContext.Database.IsRelational())
		dbContext.Database.Migrate();
});

// await app.Services.GetRequiredService<IGatewayConfigurationProvider>().RefreshProxyConfigurationAsync();
app.UseExceptionHandler();
// TODO to be tested. This will apply rate limiting to the reverse proxy only and in the correct order from a middleware perspective
// app.UseRateLimiter();
// app.MapReverseProxy().RequireRateLimiting("SlidingWindowPerClientId");
app.MapReverseProxy(proxy => { proxy.UseRateLimiter(); });
app.MapControllers();
app.MapGet("/info", () => Results.Ok(new
{
	Service = "DomainGateway",
	Version = "0.0.1",
	Description = "...."
}));
app.MapDefaultEndpoints();

if (configuration.ExposeConfigurationEndpoints)
{
	var prefix = configuration.ConfigurationsEndpointPrefix.Trim('/') + "/";
	app.MapGet($"/{prefix}setup", (IOptions<DomainGatewaySetup> options) => Results.Json(options.Value));
	app.MapGet($"/{prefix}proxy", (IGatewayConfigurationService provider) => Results.Json(provider.GetProxyConfiguration()));
	app.MapGet($"/{prefix}ratelimiter", (IGatewayConfigurationService provider) => Results.Json(provider.GetRateLimiterConfiguration()));
	app.MapGet($"/{prefix}servicediscovery", (IGatewayConfigurationService provider) => Results.Json(provider.GetServiceDiscoveryConfiguration()));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	if (configuration.EnableAdminPortal)
		app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

// app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (configuration.EnableAdminPortal)
{
	app.UseAntiforgery();
	app.MapStaticAssets();
	app.MapRazorComponents<App>()
		.AddInteractiveWebAssemblyRenderMode()
		.AddAdditionalAssemblies(typeof(DomainGateway.Client._Imports).Assembly);
}

await app.RunAsync();

// partial class is needed for integration tests to access Program class
public partial class Program
{
}
