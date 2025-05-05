using MessagingLibrary.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessagingLibrary.RabbitMq;

public class RabbitMqConsumer : IMessageConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queue;

    public RabbitMqConsumer(string hostname, string exchange, string queue, string routingKey)
    {
        _queue = queue;

        var factory = new ConnectionFactory { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: queue, exchange: exchange, routingKey: routingKey);
    }

    public Task StartConsumingAsync(Func<string, Task> handleMessage)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            await handleMessage(message);
        };

        _channel.BasicConsume(queue: _queue, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}