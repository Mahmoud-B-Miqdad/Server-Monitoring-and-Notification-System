using ServerMonitoringSystem.MessageProcessor.Configuration;
using ServerMonitoringSystem.Shared.Configuration;
using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.MessageProcessor.Services;

public class AnomalyDetector : IAnomalyDetector
{
    private readonly AnomalyDetectionConfig _anomalyConfig;
    private readonly ServerStatisticsConfig _serverConfig;
    private readonly Dictionary<string, ServerStatistics> _lastKnownStats = new();

    public AnomalyDetector(AnomalyDetectionConfig anomalyConfig, ServerStatisticsConfig serverConfig)
    {
        _anomalyConfig = anomalyConfig;
        _serverConfig = serverConfig;
    }

    public IEnumerable<string> Analyze(ServerStatistics currentStats)
    {
        var alerts = new List<string>();

        if (!_lastKnownStats.TryGetValue(_serverConfig.ServerIdentifier, out var previousStats))
        {
            _lastKnownStats[_serverConfig.ServerIdentifier] = currentStats;
            return alerts;
        }

        alerts.AddRange(AnalyzeMemoryUsage(currentStats, previousStats));
        alerts.AddRange(AnalyzeCpuUsage(currentStats, previousStats));
        alerts.AddRange(AnalyzeHighUsage(currentStats));

        _lastKnownStats[_serverConfig.ServerIdentifier] = currentStats;
        return alerts;
    }

    private IEnumerable<string> AnalyzeMemoryUsage(ServerStatistics current, ServerStatistics previous)
    {
        var alerts = new List<string>();

        if (current.MemoryUsage > previous.MemoryUsage * (1 + _anomalyConfig.MemoryUsageAnomalyThresholdPercentage))
        {
            alerts.Add("Memory Anomaly");
        }

        double memoryUsagePercent = current.MemoryUsage / (current.MemoryUsage + current.AvailableMemory);
        if (memoryUsagePercent > _anomalyConfig.MemoryUsageThresholdPercentage)
        {
            alerts.Add("High Memory Usage");
        }

        return alerts;
    }

    private IEnumerable<string> AnalyzeCpuUsage(ServerStatistics current, ServerStatistics previous)
    {
        var alerts = new List<string>();

        if (current.CpuUsage > previous.CpuUsage * (1 + _anomalyConfig.CpuUsageAnomalyThresholdPercentage))
        {
            alerts.Add("CPU Anomaly");
        }

        if (current.CpuUsage > _anomalyConfig.CpuUsageThresholdPercentage)
        {
            alerts.Add("High CPU Usage");
        }

        return alerts;
    }

    private IEnumerable<string> AnalyzeHighUsage(ServerStatistics current)
    {
        var alerts = new List<string>();

        if (current.MemoryUsage > _anomalyConfig.MemoryUsageThresholdPercentage)
        {
            alerts.Add("High Memory Usage");
        }

        if (current.CpuUsage > _anomalyConfig.CpuUsageThresholdPercentage)
        {
            alerts.Add("High CPU Usage");
        }

        return alerts;
    }
}
