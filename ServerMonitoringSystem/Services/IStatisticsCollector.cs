
using ServerMonitoringSystem.Shared.Domain;

public interface IStatisticsCollector : IDisposable
{
    ServerStatistics Collect();
}
