using System.Text.Json;
using DomainGateway.Client.Core.Models;
using SoftEEring.Core.Helpers;

namespace DomainGateway.ServiceDiscovery;

public static class ServiceInstanceExtensions 
{
	extension(ServiceInstance si)
	{
		public ServiceInstanceEntity ToServiceInstanceEntity()
		{
			return new ServiceInstanceEntity
			{
				ServiceName = si.ServiceName,
				InstanceId = si.InstanceId,
				ServiceVersion = si.ServiceVersion,
				Host = si.Host,
				Port = si.Port,
				MetadataValue = si.Metadata?.Let(m => JsonSerializer.Serialize(m))
			};
		}
	}
}

public class ServiceInstanceEntity
{
	public required string ServiceName { get; init; }
	public required string InstanceId { get; init; }
	public required string? ServiceVersion { get; set; }
	public required string Host { get; set; }
	public required int Port { get; set; }
	public DateTimeOffset RegistrationTime { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset LastSeenTime { get; set; } = DateTimeOffset.UtcNow;
	public string? MetadataValue { get; set; }

	public ServiceInstance ToServiceInstance()
	{
		var metadata = string.IsNullOrWhiteSpace(this.MetadataValue) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(this.MetadataValue);
		return new ServiceInstance(this.ServiceName, this.InstanceId, this.ServiceVersion, this.Host, this.Port, metadata);
	}
}
