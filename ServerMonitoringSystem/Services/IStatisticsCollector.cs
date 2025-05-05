using ServerMonitoringSystem.Domain;

public interface IStatisticsCollector
{
    ServerStatistics Collect();
}
