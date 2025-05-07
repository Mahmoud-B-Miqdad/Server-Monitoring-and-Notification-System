using ServerMonitoringSystem.Configuration;
using MessagingLibrary.Interfaces;
using ServerMonitoringSystem.Domain;

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

            _publisher.Publish(topic, stats);

            return stats;
        }
    }
}
