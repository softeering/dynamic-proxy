using DomainGateway.Configurations;

namespace DomainGateway.Contracts;

public interface IGatewayConfigurationProvider
{
	ProxyConfig GetProxyConfiguration();
	Task<ProxyConfig> LoadProxyConfigurationAsync(CancellationToken cancellationToken = default);
	Task RefreshProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default);
	Task SaveProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default);

	RateLimiterConfiguration GetRateLimiterConfiguration();
	Task<RateLimiterConfiguration> LoadRateLimiterConfigurationAsync(CancellationToken cancellationToken = default);
	Task RefreshRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default);
	Task SaveRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default);

	ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration();
	Task<ServiceDiscoveryConfiguration> LoadServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default);
	Task RefreshServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default);
	Task SaveServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default);
}
