namespace ServerMonitoringSystem.Shared.Configuration;

public class RabbitMqConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Exchange { get; set; }
    public string ExchangeType { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RoutingKey { get; set; }
    public string Queue { get; set; }
}