using DynamicProxy.ConfigurationProviders.AwsS3;
using DynamicProxy.Models;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using DynamicProxy.ConfigurationProviders.FileSystem;
using DynamicProxy.Infrastructure;
using DynamicProxy.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
var configurationSection = builder.Configuration.GetSection("DynamicProxyConfiguration");
builder.Services.Configure<DynamicProxySetup>(configurationSection);
var configuration = configurationSection.Get<DynamicProxySetup>()!;

// setup observability
builder.Services.AddOpenTelemetry()
	.WithTracing(tracing =>
	{
		// tracing
		// 	.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Constants.ApplicationName))
		// 	.AddAspNetCoreInstrumentation()
		// 	.AddHttpClientInstrumentation()
		// 	.AddOtlpExporter(options =>
		// 	{
		// 		options.Endpoint = new Uri("http://localhost:4317"); // Datadog Agent OTLP endpoint
		// 	});
	})
	.WithMetrics(metrics =>
	{
		// metrics
		// 	.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Constants.ApplicationName))
		// 	.AddAspNetCoreInstrumentation()
		// 	.AddHttpClientInstrumentation()
		// 	.AddOtlpExporter(options => { options.Endpoint = new Uri("http://localhost:4317"); });
	});

// Add rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
	options.AddPolicy("SlidingWindowPerClientId", context =>
	{
		// Extract clientid from header, fallback unknown if not present
		var clientId = context.Request.Headers[configuration.ClientIdHeaderName].FirstOrDefault() ?? "unknown";
		var configProvider = context.RequestServices.GetRequiredService<IGatewayConfigurationProvider>();
		var config = configProvider.GetThrottlingConfiguration(clientId);
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
		var clientId = context.HttpContext.Request.Headers[configuration.ClientIdHeaderName].FirstOrDefault() ?? "unknown";
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
else
{
	throw new Exception("No Gateway configuration provider provided");
}

builder.Services.AddReverseProxy();

builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();

// Use rate limiting middleware globally
app.UseRateLimiter();

app.MapReverseProxy().RequireRateLimiting("SlidingWindowPerClientId");

app.MapGet("/config", (IOptions<DynamicProxySetup> options) => Results.Json(options.Value));
app.MapGet("/test/info", () =>
{
	return Results.Ok(new
	{
		Service = "DynamicProxy",
		Version = "1.0.0",
		Description = "A dynamic proxy service with AWS S3 configuration support."
	});
});


app.MapControllers();

await app.RunAsync();
