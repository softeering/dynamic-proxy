using DomainGateway.Configurations;
using DomainGateway.Contracts;
using DomainGateway.Infrastructure;

namespace DomainGateway.ServiceDiscovery;

public class InstanceCleanUpJob(
	ILogger<InstanceCleanUpJob> logger,
	DomainGatewaySetup gatewaySetup,
	IGatewayConfigurationProvider configurationProvider,
	IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(gatewaySetup.AutomaticServiceInstanceCleanupInterval);

		while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
		{
			var configuration = configurationProvider.GetServiceDiscoveryConfiguration();
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
	}
}
