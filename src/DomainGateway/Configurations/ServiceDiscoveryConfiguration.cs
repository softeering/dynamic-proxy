using System.Text.Json.Serialization;

namespace DomainGateway.Configurations;

public class ServiceDiscoveryConfiguration
{
	public static readonly ServiceDiscoveryConfiguration Default = new();
	
	public TimeSpan AutomaticCleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
	public List<string> AllowedClients { get; set; }
	public List<RegisteredService> RegisteredServices { get; set; }
}

public class RegisteredService
{
	public string Name { get; init; }
	public string Description { get; init; }
	public string OwnerEmail { get; init; }
	public TimeSpan InstancesHeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);
	public int MaxHeartbeatIntervalMiss { get; set; } = 3;
	
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public AutoDeregistration AutoDeregistration { get; init; }
	
	public DateTimeOffset InstanceStaleAt => DateTimeOffset.UtcNow.Add(-InstancesHeartbeatInterval * MaxHeartbeatIntervalMiss);
}

public enum AutoDeregistration
{
	Disabled,
	DryRun, // Run but do not deregister, only log the intent
	Enabled
}
