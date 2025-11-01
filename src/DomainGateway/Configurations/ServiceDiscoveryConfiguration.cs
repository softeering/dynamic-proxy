using System.Text.Json.Serialization;

namespace DomainGateway.Configurations;

public class ServiceDiscoveryConfiguration
{
	public static readonly ServiceDiscoveryConfiguration Default = new();

	public int MaxHeartbeatIntervalMiss { get; set; }
	public List<string> AllowedClients { get; set; }
	public List<RegisteredService> RegisteredServices { get; set; }
}

public class RegisteredService
{
	public string Name { get; init; }
	public string Description { get; init; }
	public string OwnerEmail { get; init; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public AutoDeregistration AutoDeregistration { get; init; }
}

public enum AutoDeregistration
{
	Disabled,
	DryRun, // Run but do not deregister, only log the intent
	Enabled
}
