using MessagingLibrary.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MessagingLibrary.RabbitMq;

public class RabbitMqPublisher : IMessagePublisher
{

    private readonly string _hostname;
    private readonly string _exchange;
    private readonly string _exchangeType;

    public RabbitMqPublisher(string hostname, string exchange, string exchangeType)
    {
        if (string.IsNullOrWhiteSpace(hostname))
            throw new ArgumentNullException(nameof(hostname), "Hostname cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(exchange))
            throw new ArgumentNullException(nameof(exchange), "Exchange cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(exchangeType))
            throw new ArgumentNullException(nameof(exchangeType), "Exchange type cannot be null or empty.");

        _hostname = hostname;
        _exchange = exchange;
        _exchangeType = exchangeType;
    }

    public void Publish<T>(string routingKey, T message)
    {
        var factory = new ConnectionFactory { HostName = _hostname };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: _exchange, type: _exchangeType, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(
            exchange: _exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }
}