using System.Text.Json;
using DomainGateway.Client.Core.Models;
using DomainGateway.Database;
using DomainGateway.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DomainGateway.ServiceDiscovery;

public class InstancesDatabaseRepository(ILogger<InstancesDatabaseRepository> logger, DomainGatewayDbContext database) : IServiceDiscoveryInstancesRepository
{
	public Task<List<ServiceInstanceEntity>> GetRegisteredInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		return database.Instances
			.Where(i => i.ServiceName.Equals(serviceName))
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

	public async Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		var saved = await database.Instances
			.FirstOrDefaultAsync(i => i.ServiceName.Equals(instance.ServiceName) && i.InstanceId.Equals(instance.InstanceId), cancellationToken)
			.ConfigureAwait(false);

		if (saved is null)
		{
			logger.LogWarning(
				"event=unknownInstance, Heartbeat received for unknown instance {ServiceName}/{InstanceId}. Instance will be registered as a new one.",
				instance.ServiceName, instance.InstanceId
			);
			await this.RegisterInstanceAsync(instance, cancellationToken).ConfigureAwait(false);
			return;
		}

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
