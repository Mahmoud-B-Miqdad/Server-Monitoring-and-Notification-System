using Microsoft.AspNetCore.SignalR;

namespace ServerMonitoring.SignalRHub.Hubs
{
    public class AlertHub : Hub
    {
        public async Task ReceiveAlert(string serverId, string alertType, DateTime timestamp)
        {
            Console.WriteLine($"Alert received from {serverId}: {alertType} at {timestamp}");
        }
    }
}