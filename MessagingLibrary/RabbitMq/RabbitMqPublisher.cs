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
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;

    public RabbitMqPublisher(string hostname, string exchange, string exchangeType, int port, string username, string password)
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
        _port = port;
        _username = username;
        _password = password;
    }

    public async Task PublishAsync<T>(string routingKey, T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            Port = _port,
            UserName = _username,
            Password = _password 
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: _exchange, type: _exchangeType, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            body: body);
    }
}