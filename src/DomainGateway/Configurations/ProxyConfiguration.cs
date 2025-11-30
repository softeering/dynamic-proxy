using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.Configurations;

public class ProxyConfig : IProxyConfig
{
	private readonly CancellationTokenSource _cts;

	public ProxyConfig()
	{
		this.Routes = [];
		this.Clusters = [];
		this._cts = new CancellationTokenSource();
		this.ChangeToken = new CancellationChangeToken(this._cts.Token);
	}

	public IReadOnlyList<RouteConfig> Routes { get; set; }
	public IReadOnlyList<ClusterConfig> Clusters { get; set; }
	public IChangeToken ChangeToken { get; }

	public void SignalChange()
	{
		this._cts.Cancel();
	}
}
