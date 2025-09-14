namespace DynamicProxy.Services;

public interface IProxyConfigurationProvider
{
    Task<object> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task RefreshConfigurationAsync(CancellationToken cancellationToken = default);
}