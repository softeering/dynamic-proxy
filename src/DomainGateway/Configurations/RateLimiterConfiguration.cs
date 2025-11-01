namespace DomainGateway.Configurations;

public class RateLimiterConfiguration
{
	public static readonly RateLimiterConfiguration Default = new() { DisableThrottling = true };

	public bool DisableThrottling { get; init; }
	public IDictionary<string, ThrottlingConfig> ConfigurationByClient { get; init; } = new Dictionary<string, ThrottlingConfig>();
}

public class ThrottlingConfig
{
	public static readonly ThrottlingConfig DefaultDisabledConfig = new() { Disabled = true };

	public bool Disabled { get; set; }
	public int PermitLimit { get; set; } = 100;
	public TimeSpan PermitLimitWindow { get; set; } = TimeSpan.FromSeconds(1);
	public int SegmentsPerWindow { get; set; } = 1;
	public int QueueLimit { get; set; } = 10;
}
