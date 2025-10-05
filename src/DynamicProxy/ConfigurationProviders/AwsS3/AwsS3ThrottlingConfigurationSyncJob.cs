using DynamicProxy.Models;
using DynamicProxy.Services;
using Microsoft.Extensions.Options;

namespace DynamicProxy.ConfigurationProviders.AwsS3;

public class AwsS3ThrottlingConfigurationSyncJob(
    ILogger<AwsS3ThrottlingConfigurationSyncJob> logger,
    IOptions<AwsS3RepositorySetup> options,
    IGatewayConfigurationProvider proxyConfigurationProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(options.Value.ThrottlingConfigurationRefreshInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Refreshing throttling configuration from S3 source {SourceKey}...", options.Value.ThrottlingConfigurationSourceKey);
            
            try
            {
                await proxyConfigurationProvider.RefreshThrottlingConfigurationAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
            }
        }
    }
}