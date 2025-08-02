namespace ApplicationDomain.Interfaces
{
    public interface IServerService
    {
        bool IsRunning { get; }

        string UrlPrefix { get; }

        Task StartAsync();

        Task StopAsync();
    }
}
