using DomainGateway.Configurations;
using System.Text.Json;
using DomainGateway.Contracts;
using DomainGateway.RateLimiting;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public class FileSystemGatewayConfigurationProvider(
	ILogger<FileSystemGatewayConfigurationProvider> logger,
	FileSystemRepositorySetup setup) : IGatewayConfigurationProvider
{
	// Proxy Configuration

	public Task<ProxyConfig> LoadProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("event=LoadProxyConfiguration, loading proxy configuration...");
		return LoadFile<ProxyConfig>(setup.ProxyConfigurationFilePath);
	}

	public Task SaveProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default)
	{
		var payload = JsonSerializer.Serialize(config);
		return File.WriteAllTextAsync(setup.ProxyConfigurationFilePath, payload, cancellationToken);
	}

	// Rate Limiter Configuration

	public Task<RateLimiterConfiguration> LoadRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Loading rate limiter configuration...");
		return LoadFile<RateLimiterConfiguration>(setup.RateLimiterConfigurationFilePath);
	}

	public Task SaveRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default)
	{
		var payload = JsonSerializer.Serialize(config);
		return File.WriteAllTextAsync(setup.RateLimiterConfigurationFilePath, payload, cancellationToken);
	}

	// Service Discovery Configuration

	public Task<ServiceDiscoveryConfiguration> LoadServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing service discovery configuration...");
		return LoadFile<ServiceDiscoveryConfiguration>(setup.ServiceDiscoveryConfigurationFilePath);
	}

	public Task SaveServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default)
	{
		var payload = JsonSerializer.Serialize(config);
		return File.WriteAllTextAsync(setup.ServiceDiscoveryConfigurationFilePath, payload, cancellationToken);
	}

	private static async Task<T> LoadFile<T>(string filePath)
	{
		await using var fileContent = File.OpenRead(filePath);
		return await JsonSerializer.DeserializeAsync<T>(fileContent).ConfigureAwait(false)
		       ?? throw new InvalidOperationException($"Failed to deserialize configuration from file: {filePath}");
	}
}
