namespace ServerMonitoringSystem.Shared.Configuration;

public class RabbitMqConfig
{
    public string RabbitHost { get; set; }
    public int RabbitPort { get; set; }
    public string Exchange { get; set; }
    public string ExchangeType { get; set; }
    public string RabbitUsername { get; set; }
    public string RabbitPassword { get; set; }
    public string RoutingKey { get; set; }
    public string Queue { get; set; }
}