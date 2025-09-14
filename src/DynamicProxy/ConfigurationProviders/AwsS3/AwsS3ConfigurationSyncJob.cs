using DynamicProxy.Models;
using DynamicProxy.Services;
using Microsoft.Extensions.Options;

namespace DynamicProxy.ConfigurationProviders.AwsS3;

public class AwsS3ConfigurationSyncJob(ILogger<AwsS3ConfigurationSyncJob> logger, IOptions<AwsS3Repository> options, IProxyConfigurationProvider proxyConfigurationProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(options.Value.RefreshInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await proxyConfigurationProvider.RefreshConfigurationAsync(stoppingToken).ConfigureAwait(false);
        }
    }
}