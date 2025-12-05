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

	public IProxyConfig GetConfig()
	{
		return this._proxyConfig;
	}

	public async Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing proxy configuration...");
		var newConfig = LoadFile<ProxyConfig>(setup.ProxyConfigurationFilePath);

		if (await this.ValidateConfig(newConfig))
		{
			logger.LogInformation("Proxy configuration validation succeeded.");
			Interlocked.Exchange(ref this._proxyConfig, newConfig).SignalChange();
		}
		else
		{
			logger.LogError("Proxy configuration validation failed.");
		}
	}

	private async Task<bool> ValidateConfig(ProxyConfig newConfig)
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

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return this._rateLimiterConfig;
	}

	public Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing rate limiter configuration...");
		var newConfig = LoadFile<RateLimiterConfiguration>(setup.RateLimiterConfigurationFilePath);
		Interlocked.Exchange(ref this._rateLimiterConfig, newConfig);
		return Task.CompletedTask;
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return this._serviceDiscoveryConfig;
	}

	public Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing service discovery configuration...");
		var newConfig = LoadFile<ServiceDiscoveryConfiguration>(setup.ServiceDiscoveryConfigurationFilePath);
		Interlocked.Exchange(ref this._serviceDiscoveryConfig, newConfig);
		return Task.CompletedTask;
	}

	private static T LoadFile<T>(string filePath)
	{
		using var fileContent = File.OpenRead(filePath);
		return JsonSerializer.Deserialize<T>(fileContent)!;
	}
}
