using DynamicProxy.ConfigurationProviders.AwsS3;
using DynamicProxy.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);

builder.Services.AddReverseProxy();
// .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var configurationSection = builder.Configuration.GetSection("DynamicProxyConfiguration");
builder.Services.Configure<DynamicProxyConfiguration>(configurationSection);
var configuration = configurationSection.Get<DynamicProxyConfiguration>();
builder.Services.AddSingleton(Options.Create(configuration!.AwsS3Repository!));

// Example: log the S3 bucket if present
if (configuration.AwsS3Repository is not null)
{
    // TODO add some logging
    builder.Services.AddS3Yarp(configuration.AwsS3Repository);
}

var app = builder.Build();

app.MapReverseProxy();

app.MapGet("/config", (IOptions<DynamicProxyConfiguration> options) => Results.Json(options.Value));

await app.RunAsync();
