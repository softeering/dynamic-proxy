using DynamicProxy.Models;

namespace DynamicProxy.Services;

public interface IGatewayConfigurationProvider
{
    ProxyConfig GetProxyConfiguration();
    Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default);
    ThrottlingConfig GetThrottlingConfiguration(string clientId);
    Task RefreshThrottlingConfigurationAsync(CancellationToken cancellationToken = default);
}