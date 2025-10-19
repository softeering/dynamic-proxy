namespace DomainGateway.ServiceDiscovery;

public interface IServiceDiscoveryInstancesRepository
{
	string ProviderName { get; }
	Task<List<ServiceInstance>> GetRegisteredInstancesAsync(string serviceName, CancellationToken cancellationToken = default);
	Task RegisterInstanceAsync(ServiceInstance instance, CancellationToken cancellationToken = default);
	Task DeregisterInstanceAsync(string instanceId, CancellationToken cancellationToken = default);
	Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default);
	Task<int> GetInstanceCountAsync(string serviceName, CancellationToken cancellationToken = default);
}
