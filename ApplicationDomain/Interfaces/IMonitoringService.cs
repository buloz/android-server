public interface IMonitoringService
{
    event Action<string> ClientConnected;
    event Action<string> ClientDisconnected;
    event Action<string, LogLevel> LogAdded;
    event Action<bool> ServerStatusChanged;
    event Action<string> ServerUrlChanged;
    
    bool IsServerOnline { get; }
    string ServerUrl { get; }
    int ConnectedClientsCount { get; }
    
    void NotifyClientConnected(string clientId);
    void NotifyClientDisconnected(string clientId);
    void AddLog(string message, LogLevel level);
    void SetServerStatus(bool isOnline);
    void SetServerUrl(string url);
}