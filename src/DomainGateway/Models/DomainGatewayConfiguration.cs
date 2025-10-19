namespace DomainGateway.Models;

public class DomainGatewaySetup
{
	public required string ClientIdHeaderName { get; init; }
	public bool ExposeConfigurationsEndpoint { get; init; }
	public string ConfigurationsEndpointPrefix { get; init; } = "/configurations";
	public AwsS3RepositorySetup? AwsS3Repository { get; init; }
	public FileSystemRepositorySetup? FileSystemRepository { get; init; }
}

public class FileSystemRepositorySetup
{
	public TimeSpan ConfigurationsRefreshInterval { get; init; } = TimeSpan.FromSeconds(30);
	
	public required string ProxyConfigurationFilePath { get; init; }
	public required string RateLimiterConfigurationFilePath { get; init; }
	public required string ServiceDiscoveryConfigurationFilePath { get; init; }
}

public class AwsS3RepositorySetup
{
	public required string Region { get; init; }
	public required string Bucket { get; init; }
	public TimeSpan ConfigurationsRefreshInterval { get; init; } = TimeSpan.FromSeconds(30);
	
	public required string ProxyConfigurationFileKey { get; init; }
	public string ProxyConfigurationSourceKey => $"{this.Bucket}/{this.ProxyConfigurationFileKey}";
	
	public required string RateLimiterConfigurationFileKey { get; init; }
	public string RateLimiterConfigurationSourceKey => $"{this.Bucket}/{this.RateLimiterConfigurationFileKey}";
	
	public required string ServiceDiscoveryConfigurationFileKey { get; init; }
	public string ServiceDiscoveryConfigurationSourceKey => $"{this.Bucket}/{this.ServiceDiscoveryConfigurationFileKey}";
}
