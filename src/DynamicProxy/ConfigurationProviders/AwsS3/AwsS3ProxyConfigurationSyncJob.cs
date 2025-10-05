using DynamicProxy.Models;
using DynamicProxy.Services;
using Microsoft.Extensions.Options;

namespace DynamicProxy.ConfigurationProviders.AwsS3;

public class AwsS3ProxyConfigurationSyncJob(
    ILogger<AwsS3ProxyConfigurationSyncJob> logger,
    IOptions<AwsS3RepositorySetup> options,
    IGatewayConfigurationProvider proxyConfigurationProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(options.Value.ProxyConfigurationRefreshInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Refreshing proxy configuration from S3 source {SourceKey}...", options.Value.ProxyConfigurationSourceKey);
            
            try
            {
                await proxyConfigurationProvider.RefreshProxyConfigurationAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
            }
        }
    }
}