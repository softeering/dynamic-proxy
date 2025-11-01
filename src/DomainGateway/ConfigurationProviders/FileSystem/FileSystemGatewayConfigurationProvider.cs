using DomainGateway.Configurations;

namespace DomainGateway.ConfigurationProviders.FileSystem;

using System.Text.Json;
using Contracts;
using Infrastructure;
using Yarp.ReverseProxy.Configuration;

public class FileSystemGatewayConfigurationProvider(
	ILogger<FileSystemGatewayConfigurationProvider> logger,
	FileSystemRepositorySetup setup) : IGatewayConfigurationProvider, IProxyConfigProvider
{
	private readonly AtomicReference<ProxyConfig> _proxyConfig = new(ProxyConfig.Default);
	private readonly AtomicReference<RateLimiterConfiguration> _rateLimiterConfig = new(RateLimiterConfiguration.Default);
	private readonly AtomicReference<ServiceDiscoveryConfiguration> _serviceDiscoveryConfig = new(ServiceDiscoveryConfiguration.Default);
	
	public ProxyConfig GetProxyConfiguration()
	{
		return this._proxyConfig.Value;
	}

	public Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		this._proxyConfig.Value = JsonSerializer.Deserialize<ProxyConfig>(File.ReadAllText(setup.ProxyConfigurationFilePath))!;
		return Task.CompletedTask;
	}

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return this._rateLimiterConfig.Value;
	}

	public Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		this._rateLimiterConfig.Value = JsonSerializer.Deserialize<RateLimiterConfiguration>(File.ReadAllText(setup.RateLimiterConfigurationFilePath))!;
		return Task.CompletedTask;
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return this._serviceDiscoveryConfig.Value;
	}

	public Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		this._serviceDiscoveryConfig.Value = JsonSerializer.Deserialize<ServiceDiscoveryConfiguration>(File.ReadAllText(setup.ServiceDiscoveryConfigurationFilePath))!;
		return Task.CompletedTask;
	}

	public IProxyConfig GetConfig()
	{
		return this.GetProxyConfiguration();
	}
}
