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

    public RabbitMqPublisher(string hostname, string exchange)
    {
        _exchange = exchange;

        var factory = new ConnectionFactory { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync(string routingKey, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: _exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
