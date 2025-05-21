using System;
using System.Net;
using System.Net.NetworkInformation;

namespace ET.Server
{
    public static class IPHelper
    {
        public static string GetLocalIp()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    foreach (UnicastIPAddressInformation ipAddressInfo in ipProperties.UnicastAddresses)
                    {
                        if (ipAddressInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ipAddressInfo.Address.ToString().Replace('.','_');
                        }
                    }
                }
            }

            return "Unknown";
        }
    }
}