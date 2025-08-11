using System;
using System.Collections.Generic;

public class MonitoringService : IMonitoringService
{
    public event Action<string> ClientConnected;
    public event Action<string> ClientDisconnected;
    public event Action<string, LogLevel> LogAdded;
    public event Action<bool> ServerStatusChanged;
    public event Action<string> ServerUrlChanged;

    public bool IsServerOnline { get; private set; }
    public string ServerUrl { get; private set; } = "http://localhost:8080";
    public int ConnectedClientsCount { get; private set; }

    private readonly HashSet<string> _connectedClients = new();

    public void NotifyClientConnected(string clientId)
    {
        if (_connectedClients.Add(clientId))
        {
            ConnectedClientsCount = _connectedClients.Count;
            ClientConnected?.Invoke(clientId);
            AddLog($"Client connecté: {clientId}", LogLevel.Info);
        }
    }

    public void NotifyClientDisconnected(string clientId)
    {
        if (_connectedClients.Remove(clientId))
        {
            ConnectedClientsCount = _connectedClients.Count;
            ClientDisconnected?.Invoke(clientId);
            AddLog($"Client déconnecté: {clientId}", LogLevel.Warning);
        }
    }

    public void AddLog(string message, LogLevel level)
    {
        LogAdded?.Invoke(message, level);
    }

    public void SetServerStatus(bool isOnline)
    {
        if (IsServerOnline != isOnline)
        {
            IsServerOnline = isOnline;
            ServerStatusChanged?.Invoke(isOnline);
            AddLog($"Serveur {(isOnline ? "démarré" : "arrêté")}", 
                   isOnline ? LogLevel.Info : LogLevel.Warning);
        }
    }

    public void SetServerUrl(string url)
    {
        if (ServerUrl != url)
        {
            ServerUrl = url;
            ServerUrlChanged?.Invoke(url);
            AddLog($"URL serveur: {url}", LogLevel.Info);
        }
    }
}