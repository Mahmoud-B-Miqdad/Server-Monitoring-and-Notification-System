using ServerMonitoringSystem.Configuration;
using MessagingLibrary.Interfaces;

namespace ServerMonitoringSystem.Services;

public class MonitoringService
{
    private readonly StatisticsCollector _collector;
    private readonly IMessagePublisher _publisher;
    private readonly ServerStatisticsConfig _config;

    public MonitoringService(StatisticsCollector collector, IMessagePublisher publisher, ServerStatisticsConfig config)
    {
        _collector = collector;
        _publisher = publisher;
        _config = config;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            var stats = _collector.Collect();
            var topic = $"ServerStatistics.{_config.ServerIdentifier}";

            _publisher.PublishAsync(topic, stats);

            Console.WriteLine($"Published stats for {_config.ServerIdentifier}: CPU {stats.CpuUsage}% - Memory {stats.MemoryUsage}MB - AvailableMemory {stats.AvailableMemory}MB");

            await Task.Delay(_config.SamplingIntervalSeconds * 1000);
        }
    }
}
