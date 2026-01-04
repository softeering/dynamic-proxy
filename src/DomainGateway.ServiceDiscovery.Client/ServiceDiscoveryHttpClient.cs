using System.Net.Http.Json;
using DomainGateway.Client.Core.Models;
using DomainGateway.ServiceDiscovery.Client.Contracts;

namespace DomainGateway.ServiceDiscovery.Client;

public sealed class ServiceDiscoveryHttpClient : IServiceDiscoveryClient
{
	private readonly HttpClient _httpClient;

	public ServiceDiscoveryHttpClient(HttpClient httpClient)
	{
		this._httpClient = httpClient;
	}

	public Task<List<ServiceInstance>> ListInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
	{
		return this._httpClient.GetFromJsonAsync<List<ServiceInstance>>($"api/servicediscovery/{serviceName}/instances", cancellationToken)!;
	}

	public Task RegisterAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		return this._httpClient.PostAsJsonAsync($"api/servicediscovery/{instance.ServiceName}/register", instance, cancellationToken);
	}

	public async Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		try
		{
			var pingResponse = await this._httpClient
				.PutAsJsonAsync($"api/servicediscovery/{instance.ServiceName}/ping", instance, cancellationToken)
				.ConfigureAwait(false);
			pingResponse.EnsureSuccessStatusCode();
		}
		catch (Exception exception)
		{
			// Swallow exceptions for ping failures
		}
	}

	public Task DeregisterAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default)
	{
		return this._httpClient.DeleteAsync($"api/servicediscovery/{serviceName}/deregister/{instanceId}", cancellationToken);
	}
}
