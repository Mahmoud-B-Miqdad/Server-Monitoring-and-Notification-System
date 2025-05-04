using System.Diagnostics;
using ServerMonitoringSystem.Domain;

namespace ServerMonitoringSystem.Services;

public class StatisticsCollector
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _availableMemoryCounter;
    private readonly PerformanceCounter _usedMemoryCounter;

    public StatisticsCollector()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        _usedMemoryCounter = new PerformanceCounter("Memory", "Committed Bytes");
    }

    public ServerStatistics Collect()
    {
        double cpuUsage = _cpuCounter.NextValue(); 
        System.Threading.Thread.Sleep(500); 
        cpuUsage = _cpuCounter.NextValue(); 

        double availableMemory = _availableMemoryCounter.NextValue(); 
        double committedBytes = _usedMemoryCounter.NextValue(); 

        double usedMemory = committedBytes / (1024 * 1024); 

        return new ServerStatistics
        {
            CpuUsage = cpuUsage,
            AvailableMemory = availableMemory,
            MemoryUsage = usedMemory,
            Timestamp = DateTime.Now
        };
    }
}
