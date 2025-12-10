using DomainGateway.Configurations;

namespace DomainGateway.Contracts;

public interface IGatewayConfigurationService
{
	ProxyConfig GetProxyConfiguration();
	Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default);
	RateLimiterConfiguration GetRateLimiterConfiguration();
	Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default);
	ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration();
	Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default);
}
