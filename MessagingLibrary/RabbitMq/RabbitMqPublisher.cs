using MessagingLibrary.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MessagingLibrary.RabbitMq;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
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

        _exchange = exchange;
        _exchangeType = exchangeType;

        var factory = new ConnectionFactory { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _exchange, type: exchangeType, durable: true);
    }

    public void Publish(string routingKey, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: _exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
