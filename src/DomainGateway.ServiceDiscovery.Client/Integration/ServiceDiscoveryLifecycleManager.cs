using DomainGateway.Client.Core.Models;
using DomainGateway.ServiceDiscovery.Client.Configuration;
using DomainGateway.ServiceDiscovery.Client.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DomainGateway.ServiceDiscovery.Client.Integration;

public class ServiceDiscoveryLifecycleManager : IHostedService
{
	private readonly ILogger _logger;
	private readonly ServiceDiscoveryRegistryConfiguration _configuration;
	private readonly ServiceInstance _instance;
	private readonly IServiceDiscoveryClient _client;
	private readonly IHostApplicationLifetime? _lifetime;
	private volatile bool _registered = false;

	public ServiceDiscoveryLifecycleManager(
		ILogger<ServiceDiscoveryLifecycleManager> logger,
		ServiceDiscoveryRegistryConfiguration configuration,
		ServiceInstance instance,
		IServiceDiscoveryClient client,
		IHostApplicationLifetime? lifetime = null
	)
	{
		this._logger = logger;
		this._configuration = configuration;
		this._instance = instance;
		this._client = client;
		this._lifetime = lifetime;

		if (this._lifetime is not null)
		{
			this._lifetime.ApplicationStopping.Register(OnStarted);
			this._lifetime.ApplicationStopping.Register(OnStopping);
		}
	}

	private async void OnStarted()
	{
		await this.RegisterAsync(CancellationToken.None).ConfigureAwait(false);
	}

	private async void OnStopping()
	{
		await this.DeregisterAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		if (this._lifetime is null)
		{
			await this.RegisterAsync(cancellationToken).ConfigureAwait(false);
		}

		_ = Task.Factory.StartNew(async () =>
		{
			using var timer = new PeriodicTimer(TimeSpan.FromSeconds(this._configuration.HeartBeatIntervalSeconds));

			while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
			{
				try
				{
					await this._client.PingAsync(this._instance, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception error)
				{
					this._logger.LogError(error, "Error during ping for instance {instance}", this._instance);
				}
			}
		}, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default); // .ConfigureAwait(false);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (this._lifetime is null)
		{
			await this.DeregisterAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public bool IsHealthy => this._registered;

	private async Task RegisterAsync(CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Service is starting up, registering instance {instance}...", this._instance);
		try
		{
			await this._client.RegisterAsync(this._instance, cancellationToken).ConfigureAwait(false);
			this._registered = true;
		}
		catch (Exception error)
		{
			this._logger.LogError(error, "Error during startup registration for instance {instance}", this._instance);
		}
	}

	private async Task DeregisterAsync(CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Service is shutting down, deregistering {instance}...", this._instance);
		try
		{
			await this._client.DeregisterAsync(this._instance.ServiceName, this._instance.InstanceId, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception error)
		{
			this._logger.LogError(error, "Error during shutdown deregistration for instance {instance}", this._instance);
		}
	}
}
