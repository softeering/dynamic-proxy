using DomainGateway.Models;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using DomainGateway.ConfigurationProviders.FileSystem;
using DomainGateway.Contracts;
using DomainGateway.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var configurationSection = builder.Configuration.GetSection("DomainGatewayConfiguration");
builder.Services.Configure<DomainGatewaySetup>(configurationSection);
var configuration = configurationSection.Get<DomainGatewaySetup>()!;

builder.Services.AddDbContext<DomainGatewayDbContext>(options => DomainGatewayDbContext.Configure(options, connectionString));

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

		return RateLimitPartition.GetSlidingWindowLimiter(clientId, key => new SlidingWindowRateLimiterOptions
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
	// builder.Services.AddAwsS3Provider(configuration.AwsS3Repository);
}
else
{
	throw new Exception("No Gateway configuration provider provided");
}

builder.Services.AddReverseProxy();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<DomainGatewayDbContext>();
	db.Database.Migrate();
}

app.MapDefaultEndpoints();

// Use rate limiting middleware globally
app.UseRateLimiter();

app.MapReverseProxy().RequireRateLimiting("SlidingWindowPerClientId");

if (configuration.ExposeConfigurationsEndpoint)
{
	app.MapGet($"/{configuration.ConfigurationsEndpointPrefix.TrimEnd('/')}/setup",
		(IOptions<DomainGatewaySetup> options) => Results.Json(options.Value));
	app.MapGet($"/{configuration.ConfigurationsEndpointPrefix.TrimEnd('/')}/proxy",
		(IGatewayConfigurationProvider provider) => Results.Json(provider.GetProxyConfiguration()));
	app.MapGet($"/{configuration.ConfigurationsEndpointPrefix.TrimEnd('/')}/ratelimiter",
		(IGatewayConfigurationProvider provider) => Results.Json(provider.GetRateLimiterConfiguration()));
	app.MapGet($"/{configuration.ConfigurationsEndpointPrefix.TrimEnd('/')}/servicediscovery",
		(IGatewayConfigurationProvider provider) => Results.Json(provider.GetServiceDiscoveryConfiguration()));
}

app.MapGet("/test/info", () => Results.Ok(new
{
	Service = "DomainGateway",
	Version = "0.0.1",
	Description = "...."
}));

app.MapControllers();

await app.RunAsync();
