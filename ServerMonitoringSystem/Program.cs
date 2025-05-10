using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using ServerMonitoringSystem.Services;
using ServerMonitoringSystem.Shared.Configuration;

var samplingInterval = int.Parse(Environment.GetEnvironmentVariable("SAMPLING_INTERVAL_SECONDS") ?? "10");
var serverIdentifier = Environment.GetEnvironmentVariable("SERVER_IDENTIFIER") ?? "default-server";

var config = new ServerStatisticsConfig
{
    SamplingIntervalSeconds = samplingInterval,
    ServerIdentifier = serverIdentifier
};

var collector = new StatisticsCollector();

var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "ServerExchange";
var exchangeType = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE_TYPE") ?? "topic";

IMessagePublisher? publisher = null;

try
{
    publisher = new RabbitMqPublisher(hostName, exchangeName, exchangeType);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Configuration error: {ex.ParamName} - {ex.Message}");
}

var monitoringService = new MonitoringService(collector, publisher, config);

try
{
    while (true)
    {
        var stats = await monitoringService.RunAsync();

        Console.WriteLine($"[INFO] Published stats: CPU={stats.CpuUsage}%, Memory={stats.MemoryUsage}MB, Available={stats.AvailableMemory}MB");

        await Task.Delay(config.SamplingIntervalSeconds * 1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {DateTime.Now}: {ex.Message}");
}