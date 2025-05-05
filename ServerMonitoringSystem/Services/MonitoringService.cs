using MessagingLibrary.Interfaces;
using ServerMonitoringSystem.Shared.Configuration;
using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.Services;

public class MonitoringService : IMonitoringService
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

    public async Task<ServerStatistics> RunAsync()
    {
        while (true)
        {
            var stats = _collector.Collect();
            var topic = $"ServerStatistics.{_config.ServerIdentifier}";

            await _publisher.PublishAsync(topic, stats);

            return stats;
        }
    }
}
