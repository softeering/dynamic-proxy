namespace DynamicProxy.Models;

public class DynamicProxySetup
{
    public required string ClientIdHeaderName { get; init; }
    public AwsS3RepositorySetup? AwsS3Repository { get; init; }
    public FileSystemRepositorySetup? FileSystemRepository { get; init; }
}

public class FileSystemRepositorySetup
{
    public required string ProxyConfigurationFilePath { get; init; }
    public required string ThrottlingConfigurationFilePath { get; init; }
}

public class AwsS3RepositorySetup
{
    public required string Region { get; init; }
    public required string Bucket { get; init; }
    
    public required string ProxyConfigurationFileKey { get; init; }
    public TimeSpan ProxyConfigurationRefreshInterval { get; init; } = TimeSpan.FromSeconds(30);
    
    public required string ThrottlingConfigurationFileKey { get; init; }
    public TimeSpan ThrottlingConfigurationRefreshInterval { get; init; } = TimeSpan.FromSeconds(30);
    
    public string ProxyConfigurationSourceKey => $"{this.Bucket}/{this.ProxyConfigurationFileKey}";
    public string ThrottlingConfigurationSourceKey => $"{this.Bucket}/{this.ThrottlingConfigurationFileKey}";
}
