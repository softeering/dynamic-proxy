using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.Configurations;

public class ProxyConfig : IProxyConfig
{
	private readonly CancellationTokenSource _cts = new();
	public IReadOnlyList<RouteConfig> Routes { get; set; }
	public IReadOnlyList<ClusterConfig> Clusters { get; set; }
	public IChangeToken ChangeToken => new CancellationChangeToken(_cts.Token);

	public void SignalChange() => this._cts.Cancel();
}
