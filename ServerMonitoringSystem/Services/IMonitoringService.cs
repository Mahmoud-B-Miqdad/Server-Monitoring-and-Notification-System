using ServerMonitoringSystem.Domain;

public interface IMonitoringService
{
    Task<ServerStatistics> RunAsync();
}
