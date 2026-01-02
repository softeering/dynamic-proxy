namespace DomainGateway.RateLimiting;

public class RateLimiterConfiguration
{
	public const string ClientIdHeaderName = "Client-Id";
	public const string FallbackClientIdHeaderValue = "default";
	public const string SlidingWindowRateLimiterPolicyName = "SlidingWindowPerClientId";
	public static readonly RateLimiterConfiguration Default = new() { DisableThrottling = true };
	private static IDictionary<string, ThrottlingConfig> NormalizeDictionary(IDictionary<string, ThrottlingConfig> source)
	{
		return source is Dictionary<string, ThrottlingConfig> dict
			? new Dictionary<string, ThrottlingConfig>(dict, StringComparer.OrdinalIgnoreCase)
			: new Dictionary<string, ThrottlingConfig>(source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), StringComparer.OrdinalIgnoreCase);
	}
	
	public bool DisableThrottling { get; init; }

	public IDictionary<string, ThrottlingConfig> ConfigurationByClient
	{
		get;
		init => field = NormalizeDictionary(value);
	} = new Dictionary<string, ThrottlingConfig>(StringComparer.OrdinalIgnoreCase);

	public (ThrottlingConfig, ThrottlingConfig?) GetApplicableConfig(string clientId, string path)
	{
		if (this.DisableThrottling)
			return (ThrottlingConfig.DefaultDisabledConfig, null);

		var normalizedPath = path.StartsWith('/') ? path : path + "/";
		var fullKey = $"{clientId}__{normalizedPath}";

		this.ConfigurationByClient.TryGetValue(fullKey, out var clientIdPathConfig);
		this.ConfigurationByClient.TryGetValue(clientId, out var clientIdConfig);

		return (clientIdConfig ?? ThrottlingConfig.DefaultDisabledConfig, clientIdPathConfig);
	}
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
