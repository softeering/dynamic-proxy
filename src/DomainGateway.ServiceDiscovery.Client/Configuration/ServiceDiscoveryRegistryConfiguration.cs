namespace DomainGateway.ServiceDiscovery.Client.Configuration;

public record ServiceDiscoveryRegistryConfiguration(
	string ServiceName,
	int ServicePort,
	string ServiceVersion,
	int HeartBeatIntervalSeconds = 30,
	int DeregisterAfterSeconds = 5
);
