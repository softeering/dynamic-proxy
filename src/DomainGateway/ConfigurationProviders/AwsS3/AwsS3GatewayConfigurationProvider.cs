using DomainGateway.Configurations;
using Amazon.S3;
using DomainGateway.Contracts;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.AwsS3;

public class AwsS3ConfigurationProvider(
	ILogger<AwsS3ConfigurationProvider> logger,
	IConfigValidator configValidator,
	IAmazonS3 s3Client) : IGatewayConfigurationProvider, IProxyConfigProvider
{
	public ProxyConfig GetProxyConfiguration()
	{
		throw new NotImplementedException();
	}

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		throw new NotImplementedException();
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		throw new NotImplementedException();
	}

	public Task<ProxyConfig> LoadProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task RefreshProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task SaveProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<RateLimiterConfiguration> LoadRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task RefreshRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task SaveRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<ServiceDiscoveryConfiguration> LoadServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task RefreshServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task SaveServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IProxyConfig GetConfig()
	{
		throw new NotImplementedException();
	}
}
