using DomainGateway.Contracts;
using DomainGateway.Models;
using Microsoft.Extensions.Options;

namespace DomainGateway.ConfigurationProviders.FileSystem;

public class FileSystemGatewayConfigurationSyncJob(
	ILogger<FileSystemGatewayConfigurationSyncJob> logger,
	IOptions<FileSystemRepositorySetup> options,
	IGatewayConfigurationProvider proxyConfigurationProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(options.Value.ConfigurationsRefreshInterval);

		do
		{
			await this.RefreshProxyConfiguration(stoppingToken).ConfigureAwait(false);
			await this.RefreshRateLimiter(stoppingToken).ConfigureAwait(false);
			await this.RefreshServiceDiscovery(stoppingToken).ConfigureAwait(false);
		} while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
	}

	private async Task RefreshProxyConfiguration(CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=ProxyConfigurationRefresh, starting refresh for proxy configuration from source {SourceKey}...",
				options.Value.ProxyConfigurationFilePath);
			await proxyConfigurationProvider.RefreshProxyConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=ProxyConfigurationRefresh, refresh completed successfully for proxy configuration from source {SourceKey}...",
				options.Value.ProxyConfigurationFilePath);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=ProxyConfigurationRefresh, refresh failed for proxy configuration from source {SourceKey}...",
				options.Value.ProxyConfigurationFilePath);
		}
	}

	private async Task RefreshRateLimiter(CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=RateLimiterRefresh, starting refresh for rate limiter configuration from source {SourceKey}...",
				options.Value.RateLimiterConfigurationFilePath);
			await proxyConfigurationProvider.RefreshRateLimiterConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=RateLimiterRefresh, refresh completed successfully for rate limiter configuration from source {SourceKey}...",
				options.Value.RateLimiterConfigurationFilePath);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=RateLimiterRefresh, refresh failed for rate limiter configuration from source {SourceKey}...",
				options.Value.RateLimiterConfigurationFilePath);
		}
	}

	private async Task RefreshServiceDiscovery(CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=ServiceDiscoveryRefresh, starting refresh for service discovery configuration from source {SourceKey}...",
				options.Value.ServiceDiscoveryConfigurationFilePath);
			await proxyConfigurationProvider.RefreshServiceDiscoveryConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=ServiceDiscoveryRefresh, refresh completed successfully for service discovery configuration from source {SourceKey}...",
				options.Value.ServiceDiscoveryConfigurationFilePath);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=ServiceDiscoveryRefresh, refresh failed for service discovery configuration from source {SourceKey}...",
				options.Value.ServiceDiscoveryConfigurationFilePath);
		}
	}
}
