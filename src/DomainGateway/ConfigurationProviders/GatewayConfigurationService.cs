using DomainGateway.Configurations;
using DomainGateway.Contracts;
using DomainGateway.Infrastructure;
using DomainGateway.ServiceDiscovery;
using SoftEEring.Core.Helpers;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.ConfigurationProviders;

static class Extensions
{
	private const string ServiceDiscoveryMetadataKey = "ServiceDiscoveryTarget";

	extension(IReadOnlyList<ClusterConfig> clusters)
	{
		public IDictionary<string, List<ClusterConfig>> GetServiceDiscoveryBasedClusters()
		{
			return clusters // .Where(c => c.Metadata?.ContainsKey(ServiceDiscoveryMetadataKey) == true)
				.GroupBy(cluster => cluster.Metadata?[ServiceDiscoveryMetadataKey])
				.Where(group => group.Key is not null)
				.ToDictionary(g => g.Key, g => g.ToList());
		}
	}
}

public class GatewayConfigurationService(
	ILogger<GatewayConfigurationService> logger,
	IConfigValidator configValidator,
	IServiceProvider serviceProvider) : IGatewayConfigurationService, IProxyConfigProvider
{
	private volatile ProxyConfig _proxyConfig = new();
	private volatile RateLimiterConfiguration _rateLimiterConfig = RateLimiterConfiguration.Default;
	private volatile ServiceDiscoveryConfiguration _serviceDiscoveryConfig = ServiceDiscoveryConfiguration.Default;

	public ProxyConfig GetProxyConfiguration()
	{
		return this._proxyConfig;
	}

	public IProxyConfig GetConfig()
	{
		return this._proxyConfig;
	}

	public async Task RefreshProxyConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing proxy configuration...");

		using var scope = serviceProvider.GetScopedService(
			out IGatewayConfigurationProvider gatewayConfigurationProvider,
			out IServiceDiscoveryInstancesRepository serviceDiscoveryRepository
		);
		var config = await gatewayConfigurationProvider.LoadProxyConfigurationAsync(cancellationToken).ConfigureAwait(false);

		if (await this.ValidateProxyConfiguration(config))
		{
			var updatedClusters = config.Clusters.ToDictionary(cc => cc.ClusterId);
			var serviceDiscoveryBasedClusters = config.Clusters.GetServiceDiscoveryBasedClusters();

			if (serviceDiscoveryBasedClusters.Any())
			{
				foreach (var (serviceName, clusterConfigs) in serviceDiscoveryBasedClusters)
				{
					// each of this service relies on service discovery to retrieve its destinations
					var instances = await serviceDiscoveryRepository.GetRegisteredInstancesAsync(serviceName, cancellationToken).ConfigureAwait(false);
					foreach (var cluster in clusterConfigs)
					{
						var destinationTemplate = cluster.Destinations?.FirstOrDefault();
						if (destinationTemplate is null)
						{
							logger.LogError(
								"YARP Cluster config '{cluster}' for service discovery '{serviceName}' does not have a destination template defined.",
								cluster.ClusterId,
								serviceName
							);
						}
						else
						{
							var destinationAddressTemplate = destinationTemplate.Value.Value.Address;

							var destinations = instances
								.Select((instance, index) =>
									(
										$"instance{index:001}",
										new DestinationConfig()
										{
											Address = destinationAddressTemplate
												.Replace("{{HOST}}", instance.Host)
												.Replace("{{PORT}}", instance.Port.ToString())
										}
									)
								)
								.ToDictionary()
								.AsReadOnly();

							var newClusterConfig = cluster with { Destinations = destinations };
							updatedClusters[cluster.ClusterId] = newClusterConfig;
						}
					}
				}

				config.Clusters = updatedClusters.Values.ToList().AsReadOnly();
			}

			logger.LogInformation("Proxy configuration validation succeeded.");
			Interlocked.Exchange(ref this._proxyConfig, config).SignalChange();
		}
	}

	public RateLimiterConfiguration GetRateLimiterConfiguration()
	{
		return this._rateLimiterConfig;
	}


	public async Task RefreshRateLimiterConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing rate limiter configuration...");

		using var scope = serviceProvider.GetScopedService(out IGatewayConfigurationProvider gatewayConfigurationProvider);
		var config = await gatewayConfigurationProvider.LoadRateLimiterConfigurationAsync(cancellationToken).ConfigureAwait(false);

		Interlocked.Exchange(ref this._rateLimiterConfig, config);
	}

	public ServiceDiscoveryConfiguration GetServiceDiscoveryConfiguration()
	{
		return this._serviceDiscoveryConfig;
	}

	public async Task RefreshServiceDiscoveryConfigurationAsync(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Refreshing service discovery configuration...");

		using var scope = serviceProvider.GetScopedService(out IGatewayConfigurationProvider gatewayConfigurationProvider);
		var config = await gatewayConfigurationProvider.LoadServiceDiscoveryConfigurationAsync(cancellationToken).ConfigureAwait(false);

		Interlocked.Exchange(ref this._serviceDiscoveryConfig, config);
	}

	private async Task<bool> ValidateProxyConfiguration(ProxyConfig newConfig)
	{
		var exceptions = new List<Exception>();

		foreach (var route in newConfig.Routes)
		{
			var errors = await configValidator.ValidateRouteAsync(route);
			exceptions.AddRange(errors);
		}

		foreach (var cluster in newConfig.Clusters)
		{
			var errors = await configValidator.ValidateClusterAsync(cluster);
			exceptions.AddRange(errors);
		}

		if (exceptions.Any())
		{
			logger.LogError("Proxy configuration validation failed with errors: {errors}", exceptions);
			return false;
		}

		return true;
	}
}
