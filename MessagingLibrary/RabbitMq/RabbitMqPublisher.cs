using MessagingLibrary.Interfaces;
using RabbitMQ.Client;
using ServerMonitoringSystem.Shared.Configuration;
using System.Text;
using System.Text.Json;

namespace MessagingLibrary.RabbitMq;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly RabbitMqConfig _config;

    public RabbitMqPublisher(RabbitMqConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.RabbitHost))
            throw new ArgumentNullException(nameof(config.RabbitHost), "Hostname cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(config.Exchange))
            throw new ArgumentNullException(nameof(config.Exchange), "Exchange cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(config.ExchangeType))
            throw new ArgumentNullException(nameof(config.ExchangeType), "Exchange type cannot be null or empty.");

        _config = config;
    }

    public async Task PublishAsync<T>(string routingKey, T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.RabbitHost,
            Port = _config.RabbitPort,
            UserName = _config.RabbitUsername,
            Password = _config.RabbitPassword
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: _config.Exchange, type: _config.ExchangeType, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: _config.Exchange,
            routingKey: routingKey,
            body: body);
    }
}