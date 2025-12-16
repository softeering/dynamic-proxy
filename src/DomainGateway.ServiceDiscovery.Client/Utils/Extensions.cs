using DomainGateway.Client.Core.Models;
using DomainGateway.ServiceDiscovery.Client.Configuration;
using DomainGateway.ServiceDiscovery.Client.Contracts;
using DomainGateway.ServiceDiscovery.Client.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DomainGateway.ServiceDiscovery.Client.Utils;

public static class Extensions
{
	public static IServiceCollection AddServiceDiscoveryClient(
		this IServiceCollection services,
		ServiceDiscoveryClientConfiguration configuration
	)
	{
		// TODO add support for automatic retry policies, circuit breakers, etc...

		services.AddHttpClient<IServiceDiscoveryClient, ServiceDiscoveryHttpClient>()
			.ConfigureHttpClient(client =>
			{
				client.BaseAddress = new Uri(configuration.Host.TrimEnd('/'));
				client.DefaultRequestHeaders.Add("Client-ID", configuration.ClientId);
				client.DefaultRequestHeaders.Add("Content-Type", "application/json");
				client.DefaultRequestHeaders.Add("Accept", "application/json");
				client.Timeout = TimeSpan.FromSeconds(configuration.HttpTimeoutSeconds ?? ServiceDiscoveryClientConfiguration.DefaultHttpTimeoutSeconds);
			});

		return services;
	}

	public static IServiceCollection AddServiceDiscoveryRegistry(
		this IServiceCollection services,
		ServiceDiscoveryRegistryConfiguration configuration,
		string? instanceId = null
	)
	{
		var address = HostHelper.GetLocalIPv4();

		if (address is null)
			throw new Exception("Host address not defined");

		var finalInstanceId = instanceId ?? Guid.NewGuid().ToString();
		
		var instance = new ServiceInstance(
			ServiceName: configuration.ServiceName,
			InstanceId: finalInstanceId,
			ServiceVersion: configuration.ServiceVersion,
			Host: address.ToString(),
			Port: configuration.ServicePort,
			Metadata: null
		);

		services.AddSingleton(instance);
		services.AddHostedService<ServiceDiscoveryLifecycleManager>();

		return services;
	}

	public static IHost UseServiceDiscoveryClient(this IHost host)
	{
		// TODO anything to add here??

		return host;
	}
}
