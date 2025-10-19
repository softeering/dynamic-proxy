namespace DomainGateway.ServiceDiscovery;

public record ServiceInstance(string InstanceId, string ServiceName, string ServiceVersion, string Host, int Port, IDictionary<string, string> Metadata);
