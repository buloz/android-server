using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class NetworkService : INetworkService
{
    public string GetIPAddress()
    {
        try
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            
            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    
                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exception
            System.Diagnostics.Debug.WriteLine($"Error getting IP: {ex.Message}");
        }
        return "Aucune adresse IP trouvée";
    }
}