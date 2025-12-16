using System.Collections.Generic;

namespace DomainGateway.Client.Core.Models;

public record ServiceInstance(string ServiceName, string InstanceId, string? ServiceVersion, string Host, int Port, IDictionary<string, string>? Metadata)
{
	public string Key => $"{ServiceName}/{InstanceId}";
	public string Address => $"{Host}:{Port}";
}
