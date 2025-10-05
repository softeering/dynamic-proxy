using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace DynamicProxy.Models;

public class ProxyConfig : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; set; }
    public IReadOnlyList<ClusterConfig> Clusters { get; set;  }
    public IChangeToken ChangeToken { get; set;  } = NullChangeToken.Singleton;
}