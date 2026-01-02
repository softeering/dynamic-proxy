using System.Text.Json;

namespace DomainGateway.Client.Core.Models;

public record ServiceInstance(string ServiceName, string InstanceId, string? ServiceVersion, string Host, int Port, IDictionary<string, string>? Metadata)
{
	public string Key => $"{ServiceName}/{InstanceId}";
	public string Address => $"{Host}:{Port}";

	public ServiceInstanceEntity ToServiceInstanceEntity()
	{
		return new ServiceInstanceEntity
		{
			ServiceName = this.ServiceName.ToLower(),
			InstanceId = this.InstanceId,
			ServiceVersion = this.ServiceVersion,
			Host = this.Host,
			Port = this.Port,
			MetadataValue = this.Metadata is null ? null : JsonSerializer.Serialize(this.Metadata)
		};
	}
}

public class ServiceInstanceEntity
{
	public string ServiceName { get; init; }
	public string InstanceId { get; init; }
	public string? ServiceVersion { get; set; }
	public string Host { get; set; }
	public int Port { get; set; }
	public DateTimeOffset RegistrationTime { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset LastSeenTime { get; set; } = DateTimeOffset.UtcNow;
	public string? MetadataValue { get; set; }

	public ServiceInstance ToServiceInstance()
	{
		var metadata = string.IsNullOrWhiteSpace(this.MetadataValue) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(this.MetadataValue);
		return new ServiceInstance(this.ServiceName, this.InstanceId, this.ServiceVersion, this.Host, this.Port, metadata);
	}
}
