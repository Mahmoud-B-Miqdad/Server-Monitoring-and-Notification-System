using MessagingLibrary.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessagingLibrary.RabbitMq;

public class RabbitMqConsumer : IMessageConsumer, IDisposable
{
    private readonly string _queue;
    private readonly string _hostname;
    private readonly string _exchange;
    private readonly string _routingKey;
    private IConnection _connection;
    private IChannel _channel;

    public RabbitMqConsumer(string hostname, string exchange, string queue, string routingKey)
    {
        _queue = queue;
        _hostname = hostname;
        _exchange = exchange;
        _routingKey = routingKey;
    }

    public async Task StartConsumingAsync(Func<string, Task> handleMessage)
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync(queue: _queue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(queue: _queue, exchange: _exchange, routingKey: _routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await handleMessage(message);
        };

        await _channel.BasicConsumeAsync(queue: _queue, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
