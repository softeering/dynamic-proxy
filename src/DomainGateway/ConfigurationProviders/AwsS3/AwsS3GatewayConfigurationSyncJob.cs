using DomainGateway.Contracts;
using DomainGateway.Models;
using Microsoft.Extensions.Options;

namespace DomainGateway.ConfigurationProviders.AwsS3;

public class AwsS3ProxyConfigurationSyncJob(
	ILogger<AwsS3ProxyConfigurationSyncJob> logger,
	IOptions<AwsS3RepositorySetup> options,
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
			logger.LogInformation("event=ProxyConfigurationRefresh, starting refresh for proxy configuration from S3 source {SourceKey}...",
				options.Value.ProxyConfigurationSourceKey);
			await proxyConfigurationProvider.RefreshProxyConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=ProxyConfigurationRefresh, refresh completed successfully for proxy configuration from S3 source {SourceKey}...",
				options.Value.ProxyConfigurationSourceKey);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=ProxyConfigurationRefresh, refresh failed for proxy configuration from S3 source {SourceKey}...",
				options.Value.ProxyConfigurationSourceKey);
		}
	}

	private async Task RefreshRateLimiter(CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=RateLimiterRefresh, starting refresh for rate limiter configuration from S3 source {SourceKey}...",
				options.Value.RateLimiterConfigurationSourceKey);
			await proxyConfigurationProvider.RefreshRateLimiterConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=RateLimiterRefresh, refresh completed successfully for rate limiter configuration from S3 source {SourceKey}...",
				options.Value.RateLimiterConfigurationSourceKey);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=RateLimiterRefresh, refresh failed for rate limiter configuration from S3 source {SourceKey}...",
				options.Value.RateLimiterConfigurationSourceKey);
		}
	}

	private async Task RefreshServiceDiscovery(CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=ServiceDiscoveryRefresh, starting refresh for service discovery configuration from S3 source {SourceKey}...",
				options.Value.ServiceDiscoveryConfigurationSourceKey);
			await proxyConfigurationProvider.RefreshServiceDiscoveryConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation(
				"event=ServiceDiscoveryRefresh, refresh completed successfully for service discovery configuration from S3 source {SourceKey}...",
				options.Value.ServiceDiscoveryConfigurationSourceKey);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=ServiceDiscoveryRefresh, refresh failed for service discovery configuration from S3 source {SourceKey}...",
				options.Value.ServiceDiscoveryConfigurationSourceKey);
		}
	}
}
