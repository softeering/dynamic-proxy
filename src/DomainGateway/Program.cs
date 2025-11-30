using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using DomainGateway.Components;
using DomainGateway.ConfigurationProviders.AwsS3;
using DomainGateway.ConfigurationProviders.AzBlobStorage;
using DomainGateway.ConfigurationProviders.FileSystem;
using DomainGateway.Configurations;
using DomainGateway.Contracts;
using DomainGateway.Database;
using DomainGateway.Infrastructure;
using DomainGateway.ServiceDiscovery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SoftEEring.Core.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults().Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
var configurationSection = builder.Configuration.GetSection("DomainGatewayConfiguration");
builder.Services.Configure<DomainGatewaySetup>(configurationSection);
var configuration = configurationSection.Get<DomainGatewaySetup>()!;

builder.Services.AddDbContext<DomainGatewayDbContext>();

// Add rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
	options.AddPolicy("SlidingWindowPerClientId", context =>
	{
		// Extract clientId from header, fallback unknown if not present
		var clientId = context.Request.Headers[configuration.ClientIdHeaderName].FirstOrDefault() ?? "unknown";
		var configProvider = context.RequestServices.GetRequiredService<IGatewayConfigurationProvider>();
		var config = configProvider.GetRateLimiterConfiguration().ConfigurationByClient.TryGetValue(clientId, out var configValue)
			? configValue
			: ThrottlingConfig.DefaultDisabledConfig;

		if (config.Disabled)
			return RateLimitPartition.GetNoLimiter(clientId);

		return RateLimitPartition.GetSlidingWindowLimiter(clientId, _ => new SlidingWindowRateLimiterOptions
		{
			PermitLimit = config.PermitLimit,
			Window = config.PermitLimitWindow,
			SegmentsPerWindow = config.SegmentsPerWindow,
			QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
			QueueLimit = config.QueueLimit
		});
	});

	options.OnRejected = (context, _) =>
	{
		// TODO add open telemetry event
		// var clientId = context.HttpContext.Request.Headers[configuration.ClientIdHeaderName].FirstOrDefault() ?? "unknown";
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
	throw new Exception("No Gateway configuration provider provided");
}

builder.Services.AddReverseProxy();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddScoped<IServiceDiscoveryInstancesRepository, InstancesDatabaseRepository>();
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(
		ClientIdHeaderRequirementHandler.PolicyName,
		policy => policy.Requirements.Add(new ClientIdHeaderRequirement(configuration.ClientIdHeaderName))
	);
});
builder.Services.AddSingleton<IAuthorizationHandler, ClientIdHeaderRequirementHandler>();
builder.Services.AddHostedService<InstanceCleanUpJob>();

var app = builder.Build();

// migrate database on startup
app.Services.GetScopedService(out DomainGatewayDbContext dbContext).Using(_ => dbContext.Database.Migrate());
// initialize configurations repository on startup
await app.Services.GetService<IGatewayConfigurationProvider>()!.RefreshProxyConfigurationAsync();
await app.Services.GetService<IGatewayConfigurationProvider>()!.RefreshRateLimiterConfigurationAsync();
await app.Services.GetService<IGatewayConfigurationProvider>()!.RefreshServiceDiscoveryConfigurationAsync();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.UseRateLimiter();
app.MapReverseProxy().RequireRateLimiting("SlidingWindowPerClientId");

if (configuration.ExposeConfigurationsEndpoint)
{
	var prefix = configuration.ConfigurationsEndpointPrefix.Trim('/') + "/";
	app.MapGet($"/{prefix}setup", (IOptions<DomainGatewaySetup> options) => Results.Json(options.Value));
	app.MapGet($"/{prefix}proxy", (IGatewayConfigurationProvider provider) => Results.Json(provider.GetProxyConfiguration()));
	app.MapGet($"/{prefix}ratelimiter", (IGatewayConfigurationProvider provider) => Results.Json(provider.GetRateLimiterConfiguration()));
	app.MapGet($"/{prefix}servicediscovery", (IGatewayConfigurationProvider provider) => Results.Json(provider.GetServiceDiscoveryConfiguration()));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveWebAssemblyRenderMode()
	.AddAdditionalAssemblies(typeof(DomainGateway.Client._Imports).Assembly);

app.MapGet("/test/info", () => Results.Ok(new
{
	Service = "DomainGateway",
	Version = "0.0.1",
	Description = "...."
}));

app.MapControllers();

await app.RunAsync();
