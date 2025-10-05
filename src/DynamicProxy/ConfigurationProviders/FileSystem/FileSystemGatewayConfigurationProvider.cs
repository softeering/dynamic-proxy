using System.Text.Json;
using DynamicProxy.Models;
using DynamicProxy.Services;
using Yarp.ReverseProxy.Configuration;

namespace DynamicProxy.ConfigurationProviders.FileSystem;

public class FileSystemGatewayConfigurationProvider(
    ILogger<FileSystemGatewayConfigurationProvider> logger,
    FileSystemRepositorySetup setup)
    : IGatewayConfigurationProvider, IProxyConfigProvider
{
    public ProxyConfig GetProxyConfiguration()
    {
        return JsonSerializer.Deserialize<ProxyConfig>(File.ReadAllText(setup.ProxyConfigurationFilePath))!;
    }

    public Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public ThrottlingConfig GetThrottlingConfiguration(string clientId)
    {
        var config = JsonSerializer.Deserialize<RateLimiterConfiguration>(File.ReadAllText(setup.ThrottlingConfigurationFilePath))!;
        return config.ConfigurationByClient[clientId];
    }

    public Task RefreshThrottlingConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public IProxyConfig GetConfig()
    {
        return this.GetProxyConfiguration();
    }
}