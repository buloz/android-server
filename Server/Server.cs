
using ApplicationDomain.Interfaces;
using EmbedIO;
using EmbedIO.Actions;
using Server.Models;
using Swan.Logging;

namespace AndroidDemo.Server;

public class ServerProvider : IServerService
{
    private CancellationTokenSource? _cancellationTokenSource;

    private WebServer? _webServer { get; set; }

    private WebSocketSubscribe? _webSocketModule;

    private readonly ServerConfiguration _conf;

    private readonly IMonitoringService _monitoringService;

    private readonly INetworkService _networkService;

    public bool IsRunning => _webServer?.State == WebServerState.Listening;

    public string UrlPrefix => _conf.UrlPrefix;

    public int ConnectedClientsCount => _webSocketModule?.ConnectedClientsCount ?? 0;

    public ServerProvider(
        IMonitoringService monitoringService,
        INetworkService networkService)
    {
        _monitoringService = monitoringService;
        _networkService = networkService;
        _conf = new ServerConfiguration();
    }

    public async Task StartAsync()
    {
        await Task.Yield();
        _cancellationTokenSource = new CancellationTokenSource();

        _webServer = CreateWebServer(UrlPrefix);
        _webServer.StateChanged += (s, e) => $"WebServer new state - {e.NewState}".Info();

        _monitoringService.SetServerStatus(true);
        _monitoringService.SetServerUrl($"{_networkService.GetIPAddress()}:{_conf.Port}");

        _ = Task.Run(async () =>
        {
            await _webServer.RunAsync(_cancellationTokenSource.Token);
        }, _cancellationTokenSource.Token);
    }

    private WebServer CreateWebServer(string url)
    {
        _webSocketModule = new WebSocketSubscribe("/ws", _monitoringService);

        return new WebServer(o => o
            .WithUrlPrefix(url)
            .WithMode(HttpListenerMode.EmbedIO))
            .WithCors(
                origins: "*", /* emulator : "10.0.2.2:8888" */ /* impossible d'ajouter plusieurs origin ? un pattern p-e ? */
                headers: "*",
                methods: "*")
            .WithModule(new ActionModule("/api/hello", HttpVerbs.Get, async ctx =>
                {
                    await ctx.SendStringAsync("Hello from EmbedIO server!", "text/plain", System.Text.Encoding.UTF8);
                }))
            .WithModule(_webSocketModule);
    }

    public async Task StopAsync()
    {
        await Task.Yield();
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        if (_webServer != null)
        {
            _webServer.Dispose();
        }
    }
}
