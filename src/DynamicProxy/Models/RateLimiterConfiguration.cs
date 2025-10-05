namespace DynamicProxy.Models;

public class RateLimiterConfiguration
{
    public bool DisableThrottling { get; set; } = false;
    public IDictionary<string, ThrottlingConfig> ConfigurationByClient { get; set; } = new Dictionary<string, ThrottlingConfig>();
}

public class ThrottlingConfig
{
    public int PermitLimit { get; set; } = 100;
    public TimeSpan PermitLimitWindow { get; set; } = TimeSpan.FromSeconds(1);
    public int SegmentsPerWindow { get; set; } = 1;
    public int QueueLimit { get; set; } = 10;
}