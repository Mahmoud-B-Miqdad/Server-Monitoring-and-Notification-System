namespace ServerMonitoringSystem.MessageProcessor.Services
{
    public interface ISignalRAlertSender
    {
        Task StartAsync();
        Task SendAlertAsync(string serverId, string alertType, DateTime timestamp);
        Task StopAsync();
    }
}