using ServerMonitoringSystem.Domain;

public interface IStatisticsCollector : IDisposable
{
    ServerStatistics Collect();
}
