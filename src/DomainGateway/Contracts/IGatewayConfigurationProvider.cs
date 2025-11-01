namespace DomainGateway.Contracts;

using DomainGateway.Configurations;

public interface IGatewayConfigurationProvider
{
    ProxyConfig GetProxyConfiguration();
    Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default);
    RateLimiterConfiguration GetRateLimiterConfiguration();
    Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default);
    ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration();
    Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default);
}
