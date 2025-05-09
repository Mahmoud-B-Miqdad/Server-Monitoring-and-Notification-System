using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.MessageProcessor.Services
{
    public interface IAnomalyDetector
    {
        IEnumerable<string> Analyze(ServerStatistics currentStats);
    }
}
