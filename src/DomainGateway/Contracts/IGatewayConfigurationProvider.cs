using DomainGateway.Configurations;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.Contracts;

public interface IGatewayConfigurationProvider
{
    IProxyConfig GetConfig();
    Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default);
    RateLimiterConfiguration GetRateLimiterConfiguration();
    Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default);
    ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration();
    Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default);
}
