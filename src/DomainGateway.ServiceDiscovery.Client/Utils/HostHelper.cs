using System.Net;
using System.Net.Sockets;

namespace DomainGateway.ServiceDiscovery.Client.Utils;

public static class HostHelper
{
	public static IPAddress? GetLocalIPv4()
	{
		string hostName = Dns.GetHostName();
		var addresses = Dns.GetHostAddresses(hostName);

		return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));
	}
}
