namespace ServerMonitoringSystem.MessageProcessor.Services.Interfaces
{
    public interface IMessageProcessor
    {
        Task<bool> StartAsync();
    }
}