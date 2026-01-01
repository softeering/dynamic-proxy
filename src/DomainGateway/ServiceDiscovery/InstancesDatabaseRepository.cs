using System.Text.Json;
using DomainGateway.Client.Core.Models;
using DomainGateway.Database;
using DomainGateway.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DomainGateway.ServiceDiscovery;

public class InstancesDatabaseRepository(ILogger<InstancesDatabaseRepository> logger, DomainGatewayDbContext database) : IServiceDiscoveryInstancesRepository
{
	public Task<List<ServiceInstanceEntity>> GetAllRegisteredInstancesAsync(CancellationToken cancellationToken = default)
	{
		return database.Instances.ToListAsync(cancellationToken);
	}

	public Task<List<ServiceInstanceEntity>> GetRegisteredInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		var serviceNameFilter = serviceName.ToLower();

		return database.Instances
			.Where(i => i.ServiceName.Equals(serviceNameFilter))
			.ToListAsync(cancellationToken);
	}

	public Task<ServiceInstanceEntity?> GetRegisteredInstanceAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default)
	{
		var serviceNameFilter = serviceName.ToLower();
		return database.Instances.FirstOrDefaultAsync(i => i.ServiceName.Equals(serviceNameFilter), cancellationToken: cancellationToken);
	}

	public async Task RegisterInstanceAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		var serviceNameFilter = instance.ServiceName.ToLower();
		var entity = instance.ToServiceInstanceEntity();

		var saved = await database.Instances
			.FirstOrDefaultAsync(i => i.ServiceName.Equals(serviceNameFilter) && i.InstanceId.Equals(instance.InstanceId), cancellationToken)
			.ConfigureAwait(false);

		if (saved is null)
		{
			database.Instances.Add(entity);
			entity.RegistrationTime = DateTimeOffset.UtcNow;
			await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
		else
		{
			logger.LogWarning("event=duplicateInstanceRegistration, Registration called for an existing instance {InstanceKey}.", instance.Key);
		}
	}

	public Task DeregisterInstanceAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default)
	{
		var serviceNameFilter = serviceName.ToLower();

		return database.Instances
			.Where(i => i.ServiceName.Equals(serviceNameFilter) && i.InstanceId.Equals(instanceId))
			.ExecuteDeleteAsync(cancellationToken);
	}

	public async Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		var serviceNameFilter = instance.ServiceName.ToLower();

		var saved = await database.Instances
			.FirstOrDefaultAsync(i => i.ServiceName.Equals(serviceNameFilter) && i.InstanceId.Equals(instance.InstanceId), cancellationToken)
			.ConfigureAwait(false);

		if (saved is null)
		{
			logger.LogWarning(
				"event=unknownInstance, Heartbeat received for unknown instance {InstanceKey}. Instance will be registered as a new one.",
				instance.Key
			);

			var entity = instance.ToServiceInstanceEntity();
			database.Instances.Add(entity);
			entity.RegistrationTime = DateTimeOffset.UtcNow;
			await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
		else
		{
			saved.ServiceVersion = instance.ServiceVersion;
			if (saved.Host != instance.Host || saved.Port != instance.Port)
			{
				logger.LogInformation(
					"event=instanceAddressChanged, Instance address changed for {ServiceName}/{InstanceId} from {OldHost}:{OldPort} to {NewHost}:{NewPort}",
					instance.ServiceName, instance.InstanceId, saved.Host, saved.Port, instance.Host, instance.Port
				);
			}

			saved.Host = instance.Host;
			saved.Port = instance.Port;
			saved.LastSeenTime = DateTimeOffset.UtcNow;
			saved.MetadataValue = instance.Metadata is null ? null : JsonSerializer.Serialize(instance.Metadata);
			await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
