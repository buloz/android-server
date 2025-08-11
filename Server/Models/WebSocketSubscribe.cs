using EmbedIO.WebSockets;
using System.Text;

namespace Server.Models
{
    internal class WebSocketSubscribe : WebSocketModule
    {
        private readonly IMonitoringService _monitoringService;

        public WebSocketSubscribe(string urlPath, IMonitoringService monitoringService) : base(urlPath, true)
        {
            _monitoringService = monitoringService;
        }

        protected override Task OnClientConnectedAsync(IWebSocketContext context)
        {
            Console.WriteLine($"Connected user: {context.Id}");
            _monitoringService.NotifyClientConnected(context.Id);

            return SendAsync(context, $"Connection succeed. Session ID: {context.Id}");
        }

        protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
        {
            Console.WriteLine($"Disconnected user: {context.Id}");
            _monitoringService.NotifyClientDisconnected(context.Id);
            
            return BroadcastAsync($"Session {context.Id} closed.");
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            var message = Encoding.UTF8.GetString(buffer);
            Console.WriteLine($"Message from user {context.Id}: {message}");
            _monitoringService.AddLog(message, LogLevel.Info);

            return ProcessMessageAsync(context, message);
        }

        private async Task ProcessMessageAsync(IWebSocketContext context, string message)
        {
            try
            {
                if (message.StartsWith("/"))
                {
                    await HandleCommandAsync(context, message);
                }
                else
                {
                    var formattedMessage = $"[{DateTime.Now:HH:mm:ss}] User {context.Id}: {message}";
                    await BroadcastAsync(formattedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed message processing: {ex.Message}");
                await SendAsync(context, $"Failed to send message: {ex.Message}");
            }
        }

        private async Task HandleCommandAsync(IWebSocketContext context, string command)
        {
            switch (command.ToLower())
            {
                case "/ping":
                    await SendAsync(context, "pong");
                    break;

                case "/time":
                    await SendAsync(context, $"Heure serveur: {DateTime.Now}");
                    break;

                case "/clients":
                    await SendAsync(context, $"Clients connectés: {ActiveContexts.Count}");
                    break;

                case "/help":
                    var helpMessage = "Commandes disponibles:\n" +
                                    "/ping - Test de connexion\n" +
                                    "/time - Heure du serveur\n" +
                                    "/clients - Nombre de clients\n" +
                                    "/help - Cette aide";
                    await SendAsync(context, helpMessage);
                    break;

                default:
                    await SendAsync(context, $"Commande inconnue: {command}");
                    break;
            }
        }

        public async Task SendMessageToClient(string contextId, string message)
        {
            var context = ActiveContexts.FirstOrDefault(c => c.Id == contextId);
            if (context != null)
            {
                await SendAsync(context, message);
            }
        }

        public async Task BroadcastMessage(string message)
        {
            await BroadcastAsync(message);
        }

        public int ConnectedClientsCount => ActiveContexts.Count;
    }
}
