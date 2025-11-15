using System.Text.Json;
using SoftEEring.Core.Helpers;

namespace DomainGateway.ServiceDiscovery;

public record ServiceInstance(string ServiceName, string InstanceId, string? ServiceVersion, string Host, int Port, IDictionary<string, string>? Metadata)
{
	public ServiceInstanceEntity ToServiceInstanceEntity()
	{
		return new ServiceInstanceEntity
		{
			ServiceName = this.ServiceName,
			InstanceId = this.InstanceId,
			ServiceVersion = this.ServiceVersion,
			Host = this.Host,
			Port = this.Port,
			MetadataValue = this.Metadata?.Let(m => JsonSerializer.Serialize(m))
		};
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
		return new ServiceInstance(this.InstanceId, this.ServiceName, this.ServiceVersion, this.Host, this.Port, metadata);
	}
}
