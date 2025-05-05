using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using Microsoft.Extensions.Configuration;
using ServerMonitoringSystem.Configuration;
using ServerMonitoringSystem.Services;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var configuration = builder.Build();

var config = configuration.GetSection("ServerStatisticsConfig").Get<ServerStatisticsConfig>();

var collector = new StatisticsCollector();
IMessagePublisher publisher = new RabbitMqPublisher("localhost", "ServerExchange");
var monitoringService = new MonitoringService(collector, publisher, config);

try
{
    while (true)
    {
        var stats = await monitoringService.RunAsync();

        Console.WriteLine(
            $"[INFO] Published stats: CPU={stats.CpuUsage}%, Memory={stats.MemoryUsage}MB, Available={stats.AvailableMemory}MB");

        await Task.Delay(config.SamplingIntervalSeconds * 1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {DateTime.Now}: {ex.Message}");
}