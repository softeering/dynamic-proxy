using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DomainGateway.ServiceDiscovery.Client.Utils;

public static class HostHelper
{
	public static IPAddress? GetLocalIPv4()
	{
		if (!NetworkInterface.GetIsNetworkAvailable())
			return null;

		IPAddress? result = null;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			// Dns.GetHostEntry fails on MacOS due to a Dns resolution failure against localhost
			result = NetworkInterface.GetAllNetworkInterfaces()
				.FirstOrDefault(n =>
					n is
					{
						OperationalStatus: OperationalStatus.Up,
						NetworkInterfaceType: NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211,
						Speed: > 0,
					}
					&& n.GetIPProperties().UnicastAddresses.Any(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork
					                                                    && addr.ToString().IndexOf(';') < 0)
				)?
				.GetIPProperties()
				.UnicastAddresses.First().Address;
		}
		else
		{
			var hostName = Dns.GetHostName();
			result = Dns.GetHostEntry(hostName)
				.AddressList
				.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork
				                      && !IPAddress.IsLoopback(ip)
				                      // not an IP v6 address
				                      && ip.ToString().IndexOf(':') < 0);
		}

#if DEBUG
		// just in case we're in debug mode (development) and we have no network at all (I'm working from the plane)
		result ??= IPAddress.Loopback;
#endif

		return result;
	}
}
