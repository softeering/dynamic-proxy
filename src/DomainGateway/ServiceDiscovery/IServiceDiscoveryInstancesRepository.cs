using DomainGateway.Client.Core.Models;

namespace DomainGateway.ServiceDiscovery;

public interface IServiceDiscoveryInstancesRepository
{
	Task<List<ServiceInstanceEntity>> GetRegisteredInstancesAsync(string serviceName, CancellationToken cancellationToken = default);
	Task RegisterInstanceAsync(ServiceInstance instance, CancellationToken cancellationToken = default);
	Task DeregisterInstanceAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default);
	Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default);
}
