using DomainGateway.Database;
using DomainGateway.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DomainGateway.ServiceDiscovery;

public class DatabaseInstancesRepository(ILogger<DatabaseInstancesRepository> logger, DomainGatewayDbContext database) : IServiceDiscoveryInstancesRepository
{
	public Task<List<ServiceInstance>> GetRegisteredInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		return database.Instances
			.Where(i => i.ServiceName.Equals(serviceName))
			.Select(i => i.ToServiceInstance())
			.ToListAsync(cancellationToken);
	}

	public Task RegisterInstanceAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		database.Instances.Add(instance.ToServiceInstanceEntity());
		return database.SaveChangesAsync(cancellationToken);
	}

	public Task DeregisterInstanceAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default)
	{
		return database.Instances
			.Where(i => i.ServiceName.Equals(serviceName) && i.InstanceId.Equals(instanceId))
			.ExecuteDeleteAsync(cancellationToken);
	}

	public Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		database.Instances.AttachAs(instance.ToServiceInstanceEntity(), EntityState.Modified);
		return database.SaveChangesAsync(cancellationToken);
	}

	public Task<int> GetInstanceCountAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		return database.Instances
			.CountAsync(i => i.ServiceName.Equals(serviceName), cancellationToken);
	}
}
