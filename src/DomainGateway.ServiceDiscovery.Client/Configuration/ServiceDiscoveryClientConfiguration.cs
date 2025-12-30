namespace DomainGateway.ServiceDiscovery.Client.Configuration;

public record ServiceDiscoveryClientConfiguration(
	string BaseUrl,
	string ClientId,
	int? HttpTimeoutSeconds = null
)
{
	public const int DefaultHttpTimeoutSeconds = 5;
}
