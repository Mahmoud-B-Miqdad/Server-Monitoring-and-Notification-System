
using ServerMonitoringSystem.Shared.Domain;

public interface IMonitoringService
{
    Task<ServerStatistics> RunAsync();
}
