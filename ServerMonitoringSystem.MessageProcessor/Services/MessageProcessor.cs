using MessagingLibrary.Interfaces;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using ServerMonitoringSystem.MessageProcessor.Services;
using ServerMonitoringSystem.MessageProcessor.Services.Interfaces;
using ServerMonitoringSystem.Shared.Configuration;
using ServerMonitoringSystem.Shared.Domain;
using System.Text.Json;

public class MessageProcessor : IMessageProcessor
{
    private readonly IStatisticsRepository _repository;
    private readonly IMessageConsumer _consumer;
    private readonly IAnomalyDetector _anomalyDetector;
    private readonly ISignalRAlertSender _notifier;
    private readonly ServerStatisticsConfig _serverConfig;

    public MessageProcessor(IStatisticsRepository repository,IAnomalyDetector anomalyDetector,
        ISignalRAlertSender notifier, IMessageConsumer consumer, ServerStatisticsConfig serverConfig)
    {
        _repository = repository;
        _consumer = consumer;
        _anomalyDetector = anomalyDetector;
        _notifier = notifier;
        _serverConfig = serverConfig;
    }

    public async Task<bool> StartAsync()
    {
        var signalRStarted = await _notifier.StartAsync();

        if (!signalRStarted)
            return false;

        await _consumer.StartConsumingAsync(async (message) =>
        {
            var stats = JsonSerializer.Deserialize<ServerStatistics>(message);
            if (stats != null)
            {
                await _repository.SaveStatisticsAsync(stats);
                var alerts = _anomalyDetector.Analyze(stats);
                foreach (var alert in alerts)
                {
                    await _notifier.SendAlertAsync(_serverConfig.ServerIdentifier, alert, stats.Timestamp);
                }
            }
        });
        return true;
    }
}