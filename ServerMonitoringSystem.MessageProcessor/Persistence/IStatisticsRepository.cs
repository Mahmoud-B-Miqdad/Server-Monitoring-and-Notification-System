using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.MessageProcessor.Persistence;

public interface IStatisticsRepository
{
    Task SaveStatisticsAsync(ServerStatistics stats);
}