namespace DomainGateway.Models;

public class ServiceDiscoveryConfiguration
{
	public int MaxHeartbeatIntervalMiss { get; init; }
	public required StorageProvidersConfiguration Repositories { get; init; }
	public required List<RegisteredService> RegisteredServices { get; init; }
}

public record RegisteredService(string Name, string Description, string Owner, string StorageProvider, AutoDeregistration AutoDeregistration);

public record StorageProvidersConfiguration(InMemoryProviderConfiguration? InMemory);

public record InMemoryProviderConfiguration(TimeSpan Ttl);

public enum AutoDeregistration
{
	Disabled,
	DryRun, // Run but do not deregister, only log the intent
	Enabled
}
