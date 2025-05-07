using System.Diagnostics;
using ServerMonitoringSystem.Domain;

namespace ServerMonitoringSystem.Services;

public class StatisticsCollector : IStatisticsCollector
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _availableMemoryCounter;
    private readonly PerformanceCounter _usedMemoryCounter;

    public StatisticsCollector()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        _usedMemoryCounter = new PerformanceCounter("Memory", "Committed Bytes");
        _cpuCounter.NextValue();
    }

    public ServerStatistics Collect()
    {
        var cpuUsage = _cpuCounter.NextValue(); 

        var availableMemory = _availableMemoryCounter.NextValue(); 
        var committedBytes = _usedMemoryCounter.NextValue(); 

        var usedMemory = committedBytes / (1024 * 1024); 

        return new ServerStatistics
        {
            CpuUsage = cpuUsage,
            AvailableMemory = availableMemory,
            MemoryUsage = usedMemory,
            Timestamp = DateTime.Now
        };
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _availableMemoryCounter?.Dispose();
        _usedMemoryCounter?.Dispose();

        GC.SuppressFinalize(this);
    }
}
