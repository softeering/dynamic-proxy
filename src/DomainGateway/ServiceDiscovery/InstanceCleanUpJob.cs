using DomainGateway.Configurations;
using DomainGateway.Contracts;
using DomainGateway.Infrastructure;
using Microsoft.Extensions.Options;

namespace DomainGateway.ServiceDiscovery;

public class InstanceCleanUpJob(
	ILogger<InstanceCleanUpJob> logger,
	IOptions<DomainGatewaySetup> gatewaySetup,
	IGatewayConfigurationService gatewayConfigurationService,
	IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var setup = gatewaySetup.Value;
		using var timer = new PeriodicTimer(setup.AutomaticServiceInstanceCleanupInterval);

		do
		{
			try
			{
				var configuration = gatewayConfigurationService.GetServiceDiscoveryConfiguration();
				if (!configuration.HasServiceRegistered)
				{
					logger.LogInformation("event=auto-deregistration, No services are registered for service discovery, skipping cleanup run");
					continue;
				}

				using var scope = serviceProvider.GetScopedService(out IServiceDiscoveryInstancesRepository instancesRepository);

				foreach (var service in configuration.RegisteredServices)
				{
					if (service.AutoDeregistration == AutoDeregistration.Disabled)
					{
						logger.LogInformation("event=auto-deregistration, Auto-deregistration is disabled for service {ServiceName}, skipping cleanup",
							service.Name);
						continue;
					}

					var serviceName = service.Name;
					var cleanupThreshold = service.InstanceStaleAt;
					var instances = await instancesRepository.GetRegisteredInstancesAsync(serviceName, stoppingToken);

					foreach (var instance in instances)
					{
						var expired = instance.LastSeenTime < cleanupThreshold;
						if (expired)
						{
							if (service.AutoDeregistration == AutoDeregistration.Enabled)
							{
								await instancesRepository.DeregisterInstanceAsync(serviceName, instance.InstanceId, stoppingToken);

								logger.LogInformation(
									"event=auto-deregistration, Instance {ServiceName}/{InstanceId} is stale (last seen at {LastSeenTime}) and has been deregistered",
									serviceName,
									instance.InstanceId,
									instance.LastSeenTime
								);
							}
							else if (service.AutoDeregistration == AutoDeregistration.DryRun)
							{
								logger.LogWarning(
									"event=auto-deregistration, Instance {ServiceName}/{InstanceId} is stale (last seen at {LastSeenTime}) and would have been deregistered (dry run)",
									serviceName,
									instance.InstanceId,
									instance.LastSeenTime
								);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "event=auto-deregistration, An error occurred during automatic service instance cleanup");
			}
		} while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
	}
}
