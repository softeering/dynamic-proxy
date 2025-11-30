using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DomainGateway.Client.Core.Models;

namespace DomainGateway.Client.Core
{
	public sealed class DomainGatewayClient
	{
		private readonly HttpClient _httpClient;

		private DomainGatewayClient(HttpClient httpClient, string clientId)
		{
			httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
			this._httpClient = httpClient;
		}

		public async Task<List<ServiceInstance>> ListInstances(string serviceName)
		{
			return null;
		}
	}
}
