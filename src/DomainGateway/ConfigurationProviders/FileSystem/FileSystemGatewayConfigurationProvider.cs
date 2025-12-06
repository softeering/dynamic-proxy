using DomainGateway.Configurations;
using System.Text.Json;
using DomainGateway.Contracts;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public class FileSystemGatewayConfigurationProvider(
	ILogger<FileSystemGatewayConfigurationProvider> logger,
	IConfigValidator configValidator,
	FileSystemRepositorySetup setup) : IGatewayConfigurationProvider, IProxyConfigProvider
{
	private volatile ProxyConfig _proxyConfig = new();
	private volatile RateLimiterConfiguration _rateLimiterConfig = RateLimiterConfiguration.Default;
	private volatile ServiceDiscoveryConfiguration _serviceDiscoveryConfig = ServiceDiscoveryConfiguration.Default;

	// Proxy Configuration

	public ProxyConfig GetProxyConfiguration()
	{
		return this._proxyConfig;
	}

	public Task<ProxyConfig> LoadProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("event=LoadProxyConfiguration, loading proxy configuration...");
		return Task.FromResult(LoadFile<ProxyConfig>(setup.ProxyConfigurationFilePath));
	}

	public async Task RefreshProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default)
	{
		if (await this.ValidateProxyConfiguration(config))
		{
			logger.LogInformation("Proxy configuration validation succeeded.");
			Interlocked.Exchange(ref this._proxyConfig, config).SignalChange();
		}
	}

	public Task SaveProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IProxyConfig GetConfig()
	{
		return this._proxyConfig;
	}

	// Rate Limiter Configuration

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return this._rateLimiterConfig;
	}

	public Task<RateLimiterConfiguration> LoadRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Loading rate limiter configuration...");
		var newConfig = LoadFile<RateLimiterConfiguration>(setup.RateLimiterConfigurationFilePath);
		return Task.FromResult(newConfig);
	}

	public Task RefreshRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing rate limiter configuration...");
		Interlocked.Exchange(ref this._rateLimiterConfig, config);
		return Task.CompletedTask;
	}

	public Task SaveRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	// Service Discovery Configuration

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return this._serviceDiscoveryConfig;
	}

	public Task<ServiceDiscoveryConfiguration> LoadServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing service discovery configuration...");
		var newConfig = LoadFile<ServiceDiscoveryConfiguration>(setup.ServiceDiscoveryConfigurationFilePath);
		return Task.FromResult(newConfig);
	}

	public Task RefreshServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing service discovery configuration...");
		Interlocked.Exchange(ref this._serviceDiscoveryConfig, config);
		return Task.CompletedTask;
	}

	public Task SaveServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	private static T LoadFile<T>(string filePath)
	{
		using var fileContent = File.OpenRead(filePath);
		return JsonSerializer.Deserialize<T>(fileContent)!;
	}

	private async Task<bool> ValidateProxyConfiguration(ProxyConfig newConfig)
	{
		foreach (var route in newConfig.Routes)
		{
			await configValidator.ValidateRouteAsync(route);
		}

		foreach (var cluster in newConfig.Clusters)
		{
			await configValidator.ValidateClusterAsync(cluster);
		}

		return true;
	}
}
