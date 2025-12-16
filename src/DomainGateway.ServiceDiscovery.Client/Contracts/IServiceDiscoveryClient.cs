using DomainGateway.Client.Core.Models;

namespace DomainGateway.ServiceDiscovery.Client.Contracts;

public interface IServiceDiscoveryClient
{
	Task<List<ServiceInstance>> ListInstancesAsync(string serviceName, CancellationToken cancellationToken = default);
	Task RegisterAsync(ServiceInstance instance, CancellationToken cancellationToken = default);
	Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default);
	Task DeregisterAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default);
}
