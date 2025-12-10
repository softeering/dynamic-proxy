using DomainGateway.Configurations;
using DomainGateway.Contracts;

namespace DomainGateway.ConfigurationProviders;

public class GatewayConfigurationSyncJob(
	ILogger<GatewayConfigurationSyncJob> logger,
	FileSystemRepositorySetup setup,
	IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// await Task.Delay(10_000, stoppingToken).ConfigureAwait(false); // Initial delay to allow other services to start
		using var timer = new PeriodicTimer(setup.ConfigurationsRefreshInterval);

		do
		{
			var gatewayConfigurationService = serviceProvider.GetRequiredService<IGatewayConfigurationService>();
			
			await this.RefreshProxyConfiguration(gatewayConfigurationService, stoppingToken).ConfigureAwait(false);
			await this.RefreshRateLimiterConfiguration(gatewayConfigurationService, stoppingToken).ConfigureAwait(false);
			await this.RefreshServiceDiscoveryConfiguration(gatewayConfigurationService, stoppingToken).ConfigureAwait(false);
		} while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
	}

	private async Task RefreshProxyConfiguration(IGatewayConfigurationService gatewayConfigurationService, CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=ProxyConfigurationRefresh, starting refresh for proxy configuration from source {SourceKey}...",
				setup.ProxyConfigurationFilePath);
			await gatewayConfigurationService.RefreshProxyConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=ProxyConfigurationRefresh, refresh completed successfully for proxy configuration from source {SourceKey}...",
				setup.ProxyConfigurationFilePath);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=ProxyConfigurationRefresh, refresh failed for proxy configuration from source {SourceKey}...",
				setup.ProxyConfigurationFilePath);
		}
	}

	private async Task RefreshRateLimiterConfiguration(IGatewayConfigurationService gatewayConfigurationService, CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=RateLimiterRefresh, starting refresh for rate limiter configuration from source {SourceKey}...",
				setup.RateLimiterConfigurationFilePath);
			await gatewayConfigurationService.RefreshRateLimiterConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=RateLimiterRefresh, refresh completed successfully for rate limiter configuration from source {SourceKey}...",
				setup.RateLimiterConfigurationFilePath);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=RateLimiterRefresh, refresh failed for rate limiter configuration from source {SourceKey}...",
				setup.RateLimiterConfigurationFilePath);
		}
	}

	private async Task RefreshServiceDiscoveryConfiguration(IGatewayConfigurationService gatewayConfigurationService, CancellationToken stoppingToken)
	{
		try
		{
			logger.LogInformation("event=ServiceDiscoveryRefresh, starting refresh for service discovery configuration from source {SourceKey}...",
				setup.ServiceDiscoveryConfigurationFilePath);
			await gatewayConfigurationService.RefreshServiceDiscoveryConfigurationAsync(stoppingToken).ConfigureAwait(false);
			logger.LogInformation("event=ServiceDiscoveryRefresh, refresh completed successfully for service discovery configuration from source {SourceKey}...",
				setup.ServiceDiscoveryConfigurationFilePath);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "event=ServiceDiscoveryRefresh, refresh failed for service discovery configuration from source {SourceKey}...",
				setup.ServiceDiscoveryConfigurationFilePath);
		}
	}
}
