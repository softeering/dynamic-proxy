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

		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			// Dns.GetHostEntry fails on MacOS due to a Dns resolution failure against localhost
			return NetworkInterface.GetAllNetworkInterfaces()
				.FirstOrDefault(n =>
					n is
					{
						OperationalStatus: OperationalStatus.Up,
						NetworkInterfaceType: NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211,
						Speed: > 0
					} && n.GetIPProperties().UnicastAddresses.Any(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
				)?
				.GetIPProperties()
				.UnicastAddresses.First().Address;
		}

		var hostName = Dns.GetHostName();
		return Dns.GetHostEntry(hostName)
			.AddressList
			.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
	}
}
