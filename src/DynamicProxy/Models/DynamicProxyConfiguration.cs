namespace DynamicProxy.Models;

public class DynamicProxyConfiguration
{
    public AwsS3Repository? AwsS3Repository { get; init; }
    
    public bool IsAwsS3Source => AwsS3Repository is not null;
}

public class AwsS3Repository
{
    public required string Region { get; init; }
    public required string Bucket { get; init; }
    public required string Key { get; init; }
    public TimeSpan RefreshInterval { get; init; } = TimeSpan.FromSeconds(30);
}
