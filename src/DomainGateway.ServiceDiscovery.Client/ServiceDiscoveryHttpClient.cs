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
		return this._httpClient.GetFromJsonAsync<List<ServiceInstance>>($"api/ServiceDiscovery/{serviceName}/instances", cancellationToken)!;
	}

	public Task RegisterAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		return this._httpClient.PostAsJsonAsync($"api/ServiceDiscovery/{instance.ServiceName}/register", instance, cancellationToken);
	}

	public Task PingAsync(ServiceInstance instance, CancellationToken cancellationToken = default)
	{
		return this._httpClient.PutAsJsonAsync($"api/ServiceDiscovery/{instance.ServiceName}/ping", instance, cancellationToken);
	}

	public Task DeregisterAsync(string serviceName, string instanceId, CancellationToken cancellationToken = default)
	{
		return this._httpClient.DeleteAsync($"api/ServiceDiscovery/{serviceName}/deregister/{instanceId}", cancellationToken);
	}
}
