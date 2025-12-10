using DomainGateway.Configurations;

namespace DomainGateway.Contracts;

public interface IGatewayConfigurationProvider
{
	Task<ProxyConfig> LoadProxyConfigurationAsync(CancellationToken cancellationToken = default);
	Task SaveProxyConfigurationAsync(ProxyConfig config, CancellationToken cancellationToken = default);
	
	Task<RateLimiterConfiguration> LoadRateLimiterConfigurationAsync(CancellationToken cancellationToken = default);
	Task SaveRateLimiterConfigurationAsync(RateLimiterConfiguration config, CancellationToken cancellationToken = default);
	
	Task<ServiceDiscoveryConfiguration> LoadServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default);
	Task SaveServiceDiscoveryConfigurationAsync(ServiceDiscoveryConfiguration config, CancellationToken cancellationToken = default);
}
