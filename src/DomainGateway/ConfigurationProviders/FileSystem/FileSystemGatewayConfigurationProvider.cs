using System.Text.Json;
using DomainGateway.Contracts;
using DomainGateway.Infrastructure;
using DomainGateway.Models;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public class FileSystemGatewayConfigurationProvider(
	ILogger<FileSystemGatewayConfigurationProvider> logger,
	FileSystemRepositorySetup setup) : IGatewayConfigurationProvider, IProxyConfigProvider
{
	private readonly AtomicReference<ProxyConfig> _proxyConfig = new();
	private readonly AtomicReference<RateLimiterConfiguration> _rateLimiterConfig = new();
	private readonly AtomicReference<ServiceDiscoveryConfiguration> _serviceDiscoveryConfig = new();
	
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
