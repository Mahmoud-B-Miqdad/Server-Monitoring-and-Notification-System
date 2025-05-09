namespace ServerMonitoringSystem.MessageProcessor.Services
{
    public interface ISignalRAlertSender
    {
        Task<bool> StartAsync();
        Task SendAlertAsync(string serverId, string alertType, DateTime timestamp);
        Task StopAsync();
    }
}