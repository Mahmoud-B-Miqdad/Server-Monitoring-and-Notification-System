using ServerMonitoringSystem.Shared.Domain;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ServerMonitoringSystem.Services;

public class StatisticsCollector : IStatisticsCollector
{
    public ServerStatistics Collect()
    {
        float cpuUsage = FixValue(GetCpuUsage());
        float availableMemory = FixValue(GetAvailableMemory());
        float usedMemory = FixValue(GetUsedMemory());

        return new ServerStatistics
        {
            CpuUsage = cpuUsage,
            AvailableMemory = availableMemory,
            MemoryUsage = usedMemory,
            Timestamp = DateTime.Now
        };
    }

    private float FixValue(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0 : value;
    }

    private float GetCpuUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetCpuUsageWindows();
        }
        else
        {
            return GetCpuUsageLinux();
        }
    }

    private float GetCpuUsageWindows()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        Task.Delay(500);
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

        return (float)(cpuUsageTotal * 100);
    }

    private float GetCpuUsageLinux()
    {
        var cpuLine1 = File.ReadLines("/proc/stat").FirstOrDefault();
        Task.Delay(500);
        var cpuLine2 = File.ReadLines("/proc/stat").FirstOrDefault();

        if (cpuLine1 == null || cpuLine2 == null) return 0;

        var cpu1 = cpuLine1.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(ulong.Parse).ToArray();
        var cpu2 = cpuLine2.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(ulong.Parse).ToArray();

        ulong idle1 = cpu1[3], idle2 = cpu2[3];
        ulong total1 = cpu1.Aggregate<ulong, ulong>(0, (acc, val) => acc + val);
        ulong total2 = cpu2.Aggregate<ulong, ulong>(0, (acc, val) => acc + val);

        ulong totalDiff = total2 - total1;
        ulong idleDiff = idle2 - idle1;

        return (totalDiff - idleDiff) * 100f / totalDiff;
    }

    private float GetAvailableMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetTotalMemoryInMB() - GetUsedMemoryInMB();
        }
        else
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var memFreeLine = lines.FirstOrDefault(l => l.StartsWith("MemAvailable"));
            if (memFreeLine == null) return 0;
            var parts = memFreeLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
            return float.Parse(parts[1].Trim().Split(' ')[0]) / 1024; 
        }
    }

    private float GetUsedMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetUsedMemoryInMB();
        }
        else
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var totalLine = lines.FirstOrDefault(l => l.StartsWith("MemTotal"));
            var availableLine = lines.FirstOrDefault(l => l.StartsWith("MemAvailable"));
            if (totalLine == null || availableLine == null) return 0;

            var total = float.Parse(totalLine.Split(':')[1].Trim().Split(' ')[0]);
            var available = float.Parse(availableLine.Split(':')[1].Trim().Split(' ')[0]);
            return (total - available) / 1024;
        }
    }

    private float GetTotalMemoryInMB()
    {
        var computerInfo = GC.GetGCMemoryInfo();
        return computerInfo.TotalAvailableMemoryBytes / (1024f * 1024f);
    }

    private float GetUsedMemoryInMB()
    {
        using var proc = Process.GetCurrentProcess();
        return proc.WorkingSet64 / (1024f * 1024f);
    }
}