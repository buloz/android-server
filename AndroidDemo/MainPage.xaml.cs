using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AndroidDemo;

public partial class MainPage : ContentPage
{
    private readonly IMonitoringService _monitoringService;

    public ObservableCollection<WebSocketClient> Clients { get; set; }
    public ObservableCollection<HttpEndpoint> Endpoints { get; set; }
    public ObservableCollection<LogEntry> Logs { get; set; }

    private System.Timers.Timer _refreshTimer;

    public MainPage(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;

        Clients = new ObservableCollection<WebSocketClient>();
        Endpoints = new ObservableCollection<HttpEndpoint>();
        Logs = new ObservableCollection<LogEntry>();

        InitializeComponent();
        InitializeData();
        SubscribeToEvents();
        InitializeTimer();
        UpdateUI();
    }
    
    private void SubscribeToEvents()
    {
        _monitoringService.ClientConnected += OnClientConnected;
        _monitoringService.ClientDisconnected += OnClientDisconnected;
        _monitoringService.LogAdded += OnLogAdded;
        _monitoringService.ServerStatusChanged += OnServerStatusChanged;
        _monitoringService.ServerUrlChanged += OnServerUrlChanged;
    }

    private void OnClientConnected(string clientId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var client = new WebSocketClient
            {
                ClientId = $"Client-{clientId}",
                ConnectedTime = DateTime.Now,
                LastActivity = DateTime.Now
            };
            Clients.Add(client);
            UpdateUI();
        });
    }

    private void OnClientDisconnected(string clientId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var client = Clients.FirstOrDefault(c => c.ClientId.EndsWith(clientId));
            if (client != null)
            {
                Clients.Remove(client);
                UpdateUI();
            }
        });
    }

    private void OnLogAdded(string message, LogLevel level)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var log = new LogEntry
            {
                Time = DateTime.Now,
                Message = message,
                Level = level,
                TextColor = GetLogColor(level)
            };

            Logs.Insert(0, log);
            
            while (Logs.Count > 50)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        });
    }

    private void OnServerStatusChanged(bool isOnline)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateServerStatus(isOnline);
        });
    }

    private void OnServerUrlChanged(string url)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ServerUrlLabel.Text = url;
        });
    }

    private void InitializeData()
    {
        ClientsCollectionView.ItemsSource = Clients;
        EndpointsCollectionView.ItemsSource = Endpoints;
        LogsCollectionView.ItemsSource = Logs;

        LoadSampleEndpoints();

        AddLog("Application démarrée", LogLevel.Info);
        AddLog("Serveur EmbedIO en écoute", LogLevel.Info);
    }

    private void LoadSampleEndpoints()
    {
        Endpoints.Add(new HttpEndpoint { Method = "GET", Path = "/api/status", Status = "Active", MethodColor = Colors.Green.ToHex() });
        Endpoints.Add(new HttpEndpoint { Method = "GET", Path = "/api/clients", Status = "Active", MethodColor = Colors.Green.ToHex() });
        Endpoints.Add(new HttpEndpoint { Method = "POST", Path = "/api/broadcast", Status = "Active", MethodColor = Colors.Orange.ToHex() });
        Endpoints.Add(new HttpEndpoint { Method = "GET", Path = "/api/health", Status = "Active", MethodColor = Colors.Green.ToHex() });
        Endpoints.Add(new HttpEndpoint { Method = "PUT", Path = "/api/config", Status = "Active", MethodColor = Colors.Blue.ToHex() });
        Endpoints.Add(new HttpEndpoint { Method = "DELETE", Path = "/api/disconnect", Status = "Active", MethodColor = Colors.Red.ToHex() });
    }

    private void InitializeTimer()
    {
        _refreshTimer = new System.Timers.Timer(5000);
        _refreshTimer.Elapsed += async (sender, e) => await RefreshDataAsync();
        _refreshTimer.Start();
    }

    private void UpdateUI()
    {
        ServerStateLabel.Text = _monitoringService.IsServerOnline ? "En ligne" : "Hors ligne";
        ServerUrlLabel.Text = _monitoringService.ServerUrl;
        ClientsCountLabel.Text = _monitoringService.ConnectedClientsCount.ToString();
        UpdateServerStatus(_monitoringService.IsServerOnline);
    }

    private void UpdateServerStatus(bool isOnline)
    {
        if (isOnline)
        {
            StatusIndicator.Style = (Style)Resources["OnlineIndicator"];
            StatusLabel.Text = "Serveur en ligne";
            StatusLabel.TextColor = (Color)Resources["SuccessColor"];
            ServerStateLabel.TextColor = (Color)Resources["SuccessColor"];
        }
        else
        {
            StatusIndicator.Style = (Style)Resources["OfflineIndicator"];
            StatusLabel.Text = "Serveur hors ligne";
            StatusLabel.TextColor = (Color)Resources["ErrorColor"];
            ServerStateLabel.TextColor = (Color)Resources["ErrorColor"];
        }
    }

    public void AddWebSocketClient(string clientId)
    {
        var client = new WebSocketClient
        {
            ClientId = $"Client-{clientId}",
            ConnectedTime = DateTime.Now,
            LastActivity = DateTime.Now
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Clients.Add(client);
            UpdateUI();
            AddLog($"Client connecté: {client.ClientId}", LogLevel.Info);
        });
    }

    public void RemoveWebSocketClient(string clientId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var client = Clients.FirstOrDefault(c => c.ClientId.EndsWith(clientId));
            if (client != null)
            {
                Clients.Remove(client);
                UpdateUI();
                AddLog($"Client déconnecté: {client.ClientId}", LogLevel.Warning);
            }
        });
    }

    public void AddLog(string message, LogLevel level)
    {
        var log = new LogEntry
        {
            Time = DateTime.Now,
            Message = message,
            Level = level,
            TextColor = GetLogColor(level)
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Logs.Insert(0, log);

            while (Logs.Count > 50)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        });
    }

    private string GetLogColor(LogLevel level)
    {
        var color = level switch
        {
            LogLevel.Info => (Color)Resources["PrimaryColor"],
            LogLevel.Warning => Colors.Orange,
            LogLevel.Error => (Color)Resources["ErrorColor"],
            _ => (Color)Resources["TextColor"]
        };
        return color.ToHex();
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AddLog($"Erreur lors de la mise à jour: {ex.Message}", LogLevel.Error);
            });
        }
    }
    
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;
        button.Text = "🔄 Actualisation...";

        try
        {
            await RefreshDataAsync();
            AddLog("Données actualisées manuellement", LogLevel.Info);
        }
        finally
        {
            button.Text = "🔄 Actualiser les données";
            button.IsEnabled = true;
        }
    }

    public void SetServerStatus(bool isOnline)
    {
        MainThread.BeginInvokeOnMainThread(UpdateUI);

        var status = isOnline ? "en ligne" : "hors ligne";
        AddLog($"Serveur {status}", isOnline ? LogLevel.Info : LogLevel.Error);
    }

    public void SetServerUrl(string url)
    {
        MainThread.BeginInvokeOnMainThread(UpdateUI);
        AddLog($"URL du serveur: {url}", LogLevel.Info);
    }

    public void UpdateEndpointStatus(string path, string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var endpoint = Endpoints.FirstOrDefault(e => e.Path == path);
            if (endpoint != null)
            {
                endpoint.Status = status;
                AddLog($"Endpoint {path} -> {status}", LogLevel.Info);
            }
        });
    }

    private async void OnCopyServerHostPortTapped(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync($"{_monitoringService.ServerUrl}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _monitoringService.ClientConnected -= OnClientConnected;
        _monitoringService.ClientDisconnected -= OnClientDisconnected;
        _monitoringService.LogAdded -= OnLogAdded;
        _monitoringService.ServerStatusChanged -= OnServerStatusChanged;
        _monitoringService.ServerUrlChanged -= OnServerUrlChanged;

        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
    }
}
