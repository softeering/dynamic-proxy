using DomainGateway.Configurations;
using System.Text.Json;
using DomainGateway.Contracts;
using DomainGateway.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public class FileSystemGatewayConfigurationProvider(
	ILogger<FileSystemGatewayConfigurationProvider> logger,
	FileSystemRepositorySetup setup) : IGatewayConfigurationProvider, IProxyConfigProvider
{
	private volatile ProxyConfig _proxyConfig = ProxyConfig.Default;
	private volatile RateLimiterConfiguration _rateLimiterConfig = RateLimiterConfiguration.Default;
	private volatile ServiceDiscoveryConfiguration _serviceDiscoveryConfig = ServiceDiscoveryConfiguration.Default;

	public ProxyConfig GetProxyConfiguration()
	{
		return this._proxyConfig;
	}

	public Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		var newConfig = JsonSerializer.Deserialize<ProxyConfig>(File.ReadAllText(setup.ProxyConfigurationFilePath))!;
		var oldConfig = Interlocked.Exchange(ref this._proxyConfig, newConfig);
		oldConfig.SignalChange();
		return Task.CompletedTask;
	}

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return this._rateLimiterConfig;
	}

	public Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		var newConfig = JsonSerializer.Deserialize<RateLimiterConfiguration>(File.ReadAllText(setup.RateLimiterConfigurationFilePath))!;
		Interlocked.Exchange(ref this._rateLimiterConfig, newConfig);
		return Task.CompletedTask;
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return this._serviceDiscoveryConfig;
	}

	public Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		var newConfig = JsonSerializer.Deserialize<ServiceDiscoveryConfiguration>(File.ReadAllText(setup.ServiceDiscoveryConfigurationFilePath))!;
		Interlocked.Exchange(ref this._serviceDiscoveryConfig, newConfig);
		return Task.CompletedTask;
	}

	public IProxyConfig GetConfig()
	{
		return this.GetProxyConfiguration();
	}
}
