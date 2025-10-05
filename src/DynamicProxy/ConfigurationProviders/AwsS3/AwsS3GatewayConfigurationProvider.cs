using System.Collections.Concurrent;
using Amazon.S3;
using DynamicProxy.Models;
using DynamicProxy.Services;
using Yarp.ReverseProxy.Configuration;

namespace DynamicProxy.ConfigurationProviders.AwsS3;

public class AwsS3ConfigurationProvider(
    ILogger<AwsS3ConfigurationProvider> logger,
    IAmazonS3 s3Client) : IGatewayConfigurationProvider, IProxyConfigProvider
{
    private readonly ConcurrentDictionary<string, ThrottlingConfig> _configs = new();
    private static readonly ThrottlingConfig DefaultConfig = new();

    public ProxyConfig GetProxyConfiguration()
    {
        return new ProxyConfig();
    }

    public async Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
    {
        
    }

    public ThrottlingConfig GetThrottlingConfiguration(string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));

        return this._configs.GetValueOrDefault(clientId, DefaultConfig);
    }

    public async Task RefreshThrottlingConfigurationAsync(CancellationToken cancellationToken = default)
    {
        
    }

    public IProxyConfig GetConfig()
    {
        return this.GetProxyConfiguration();
    }
}