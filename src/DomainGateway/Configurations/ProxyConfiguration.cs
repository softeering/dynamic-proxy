using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace DomainGateway.Configurations;

public class ProxyConfig : IProxyConfig
{
	public static readonly ProxyConfig Default = new();
	
    public IReadOnlyList<RouteConfig> Routes { get; set; }
    public IReadOnlyList<ClusterConfig> Clusters { get; set;  }
    public IChangeToken ChangeToken { get; set;  } = NullChangeToken.Singleton;
}
