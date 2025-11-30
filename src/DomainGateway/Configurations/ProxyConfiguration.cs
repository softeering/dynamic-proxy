using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.Configurations;

public class ProxyConfig : IProxyConfig
{
	public static readonly ProxyConfig Default = new();
	private readonly CancellationTokenSource _cts = new();

	public IReadOnlyList<RouteConfig> Routes { get; set; }
	public IReadOnlyList<ClusterConfig> Clusters { get; set; }
	public IChangeToken ChangeToken { get; set; } = NullChangeToken.Singleton;

	public void SignalChange()
	{
		this._cts.Cancel();
	}
}
