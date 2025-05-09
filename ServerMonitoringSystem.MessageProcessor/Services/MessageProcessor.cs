using MessagingLibrary.Interfaces;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using ServerMonitoringSystem.MessageProcessor.Services;
using ServerMonitoringSystem.MessageProcessor.Services.Interfaces;
using ServerMonitoringSystem.Shared.Domain;
using System.Text.Json;

public class MessageProcessor : IMessageProcessor
{
    private readonly IStatisticsRepository _repository;
    private readonly IMessageConsumer _consumer;
    private readonly IAnomalyDetector _anomalyDetector;
    private readonly ISignalRAlertSender _notifier;

    public MessageProcessor(IStatisticsRepository repository,IAnomalyDetector anomalyDetector,
        ISignalRAlertSender notifier, IMessageConsumer consumer)
    {
        _repository = repository;
        _consumer = consumer;
        _anomalyDetector = anomalyDetector;
        _notifier = notifier;
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
            }
        });
        return true;
    }
}