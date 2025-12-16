namespace DomainGateway.ServiceDiscovery.Client.Configuration;

public record ServiceDiscoveryClientConfiguration(
	string Host,
	string ClientId,
	int? HttpTimeoutSeconds = null
)
{
	public const int DefaultHttpTimeoutSeconds = 15;
}
