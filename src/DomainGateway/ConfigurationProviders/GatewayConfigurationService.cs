using DomainGateway.Configurations;
using DomainGateway.Contracts;
using DomainGateway.Infrastructure;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders;

public class GatewayConfigurationService(
	ILogger<GatewayConfigurationService> logger,
	IConfigValidator configValidator,
	IServiceProvider serviceProvider) : IGatewayConfigurationService, IProxyConfigProvider
{
	private volatile ProxyConfig _proxyConfig = new();
	private volatile RateLimiterConfiguration _rateLimiterConfig = RateLimiterConfiguration.Default;
	private volatile ServiceDiscoveryConfiguration _serviceDiscoveryConfig = ServiceDiscoveryConfiguration.Default;

	public ProxyConfig GetProxyConfiguration()
	{
		return this._proxyConfig;
	}

	public IProxyConfig GetConfig()
	{
		return this._proxyConfig;
	}

	public async Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing proxy configuration...");
		
		using var scope = serviceProvider.GetScopedService(out IGatewayConfigurationProvider gatewayConfigurationProvider);
		var config = await gatewayConfigurationProvider.LoadProxyConfigurationAsync(cancellationToken).ConfigureAwait(false);
		
		if (await this.ValidateProxyConfiguration(config))
		{
			logger.LogInformation("Proxy configuration validation succeeded.");
			Interlocked.Exchange(ref this._proxyConfig, config).SignalChange();
		}
	}

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return this._rateLimiterConfig;
	}


	public async Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing rate limiter configuration...");
		
		using var scope = serviceProvider.GetScopedService(out IGatewayConfigurationProvider gatewayConfigurationProvider);
		var config = await gatewayConfigurationProvider.LoadRateLimiterConfigurationAsync(cancellationToken).ConfigureAwait(false);
		
		Interlocked.Exchange(ref this._rateLimiterConfig, config);
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return this._serviceDiscoveryConfig;
	}
	
	public async Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing service discovery configuration...");
		
		using var scope = serviceProvider.GetScopedService(out IGatewayConfigurationProvider gatewayConfigurationProvider);
		var config = await gatewayConfigurationProvider.LoadServiceDiscoveryConfigurationAsync(cancellationToken).ConfigureAwait(false);
		
		Interlocked.Exchange(ref this._serviceDiscoveryConfig, config);
	}

	private async Task<bool> ValidateProxyConfiguration(ProxyConfig newConfig)
	{
		var exceptions = new List<Exception>();

		foreach (var route in newConfig.Routes)
		{
			var errors = await configValidator.ValidateRouteAsync(route);
			exceptions.AddRange(errors);
		}

		foreach (var cluster in newConfig.Clusters)
		{
			var errors = await configValidator.ValidateClusterAsync(cluster);
			exceptions.AddRange(errors);
		}

		if (exceptions.Any())
		{
			logger.LogError("Proxy configuration validation failed with errors: {errors}", exceptions);
			return false;
		}

		return true;
	}
}
