using MessagingLibrary.Interfaces;
using ServerMonitoringSystem.Shared.Configuration;
using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.Services;

public class MonitoringService : IMonitoringService
{
    private readonly IStatisticsCollector _collector;
    private readonly IMessagePublisher _publisher;
    private readonly ServerStatisticsConfig _config;

    public MonitoringService(IStatisticsCollector collector, IMessagePublisher publisher, ServerStatisticsConfig config)
    {
        _collector = collector;
        _publisher = publisher;
        _config = config;
    }

    public async Task<ServerStatistics> RunAsync()
    {
        var stats = _collector.Collect();
        var topic = $"ServerStatistics.{_config.ServerIdentifier}";

        try
        {
            await _publisher.PublishAsync(topic, stats);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing RabbitMq: {ex.Message}");
        }

        return stats;
    }
}