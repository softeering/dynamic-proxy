using System.Collections.Concurrent;
using Amazon.S3;
using DomainGateway.Contracts;
using DomainGateway.Models;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.AwsS3;

public class AwsS3ConfigurationProvider(
	ILogger<AwsS3ConfigurationProvider> logger,
	IAmazonS3 s3Client) : IGatewayConfigurationProvider, IProxyConfigProvider
{
	public ProxyConfig GetProxyConfiguration()
	{
		return new ProxyConfig();
	}

	public async Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
	}

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return new RateLimiterConfiguration();
	}

	public async Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return new ServiceDiscoveryConfiguration()
		{
			MaxHeartbeatIntervalMiss = 3,
			RegisteredServices = [],
			Repositories = new StorageProvidersConfiguration(null)
		};
	}

	public async Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
	}

	public IProxyConfig GetConfig()
	{
		return this.GetProxyConfiguration();
	}
}
