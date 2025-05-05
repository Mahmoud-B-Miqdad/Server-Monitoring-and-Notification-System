using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using ServerMonitoringSystem.MessageProcessor.Services.Interfaces;
using ServerMonitoringSystem.Shared.Domain;
using System.Text.Json;

public class MessageProcessor : IMessageProcessor
{
    private readonly IStatisticsRepository _repository;
    private readonly IMessageConsumer _consumer;

    public MessageProcessor(IStatisticsRepository repository, string rabbitMqHost, string exchange, string queue, string routingKey)
    {
        _repository = repository;
        _consumer = new RabbitMqConsumer(rabbitMqHost, exchange, queue, routingKey);
    }

    public async Task StartAsync()
    {
        await _consumer.StartConsumingAsync(async (message) =>
        {
            var stats = JsonSerializer.Deserialize<ServerStatistics>(message);
            if (stats != null)
            {
                await _repository.SaveStatisticsAsync(stats);
            }
        });
    }
}