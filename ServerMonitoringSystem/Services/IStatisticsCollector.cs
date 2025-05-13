
using ServerMonitoringSystem.Shared.Domain;

public interface IStatisticsCollector
{
    ServerStatistics Collect();
}
