namespace DomainGateway.Configurations;

public class DomainGatewaySetup
{
	public required string ClientIdHeaderName { get; init; }
	public bool ExposeConfigurationEndpoints { get; init; }
	public bool EnableAdminPortal { get; init; }
	public string ConfigurationsEndpointPrefix { get; init; } = "/configurations";
	public int? UnsecuredHttp2Port { get; init; }
	public TimeSpan AutomaticServiceInstanceCleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
	public FileSystemRepositorySetup? FileSystemRepository { get; init; }
	public AwsS3RepositorySetup? AwsS3Repository { get; init; }
	public AzBlobStorageRepositorySetup? AzBlobStorageRepository { get; init; }
}

public class FileSystemRepositorySetup
{
	public TimeSpan ConfigurationsRefreshInterval { get; init; } = TimeSpan.FromMinutes(5);
	
	public required string ProxyConfigurationFilePath { get; init; }
	public required string RateLimiterConfigurationFilePath { get; init; }
	public required string ServiceDiscoveryConfigurationFilePath { get; init; }
}

public class AwsS3RepositorySetup
{
	public required string Region { get; init; }
	public required string Bucket { get; init; }
	public TimeSpan ConfigurationsRefreshInterval { get; init; } = TimeSpan.FromMinutes(5);
	
	public required string ProxyConfigurationFileKey { get; init; }
	public string ProxyConfigurationSourceKey => $"{this.Bucket}/{this.ProxyConfigurationFileKey}";
	
	public required string RateLimiterConfigurationFileKey { get; init; }
	public string RateLimiterConfigurationSourceKey => $"{this.Bucket}/{this.RateLimiterConfigurationFileKey}";
	
	public required string ServiceDiscoveryConfigurationFileKey { get; init; }
	public string ServiceDiscoveryConfigurationSourceKey => $"{this.Bucket}/{this.ServiceDiscoveryConfigurationFileKey}";
}

public class AzBlobStorageRepositorySetup
{
	public required string Region { get; init; }
	public required string Bucket { get; init; }
	public TimeSpan ConfigurationsRefreshInterval { get; init; } = TimeSpan.FromMinutes(5);
	
	public required string ProxyConfigurationFileKey { get; init; }
	public string ProxyConfigurationSourceKey => $"{this.Bucket}/{this.ProxyConfigurationFileKey}";
	
	public required string RateLimiterConfigurationFileKey { get; init; }
	public string RateLimiterConfigurationSourceKey => $"{this.Bucket}/{this.RateLimiterConfigurationFileKey}";
	
	public required string ServiceDiscoveryConfigurationFileKey { get; init; }
	public string ServiceDiscoveryConfigurationSourceKey => $"{this.Bucket}/{this.ServiceDiscoveryConfigurationFileKey}";
}
