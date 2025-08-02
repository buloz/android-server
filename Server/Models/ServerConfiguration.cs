using System.Net;
using System.Net.Sockets;

namespace Server.Models
{
    internal class ServerConfiguration
    {
        public string Host => "*";

        public int Port => 8888;

        public string UrlPrefix => $"http://{Host}:{Port}/";

        public bool EnableWebSockets { get; set; } = true;

        private string GetDeviceIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork
                                   && !IPAddress.IsLoopback(ip)
                                   && ip.ToString().StartsWith("192.168."))
                ?.ToString() ?? "localhost";
        }
    }
}
