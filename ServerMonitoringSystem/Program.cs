using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using Microsoft.Extensions.Configuration;
using ServerMonitoringSystem.Services;
using ServerMonitoringSystem.Shared.Configuration;

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

    var serverConfig = ConfigurationLoader.Load<ServerStatisticsConfig>(configuration, "ServerConfig");
    var rabbitMqConfig = ConfigurationLoader.Load<RabbitMqConfig>(configuration, "RabbitMqConfig");

    var collector = new StatisticsCollector();

    IMessagePublisher? publisher = null;

    try
    {
        publisher = new RabbitMqPublisher(rabbitMqConfig);     
    }
    catch (ArgumentNullException ex)
    {
        Console.WriteLine($"Configuration error: {ex.ParamName} - {ex.Message}");
    }

    var monitoringService = new MonitoringService(collector, publisher, serverConfig);

    while (true)
    {
        try
        {
            var stats = await monitoringService.RunAsync();

            Console.WriteLine($"[INFO] Published stats: CPU={stats.CpuUsage}%, Memory={stats.MemoryUsage}MB, Available={stats.AvailableMemory}MB");

            await Task.Delay(serverConfig.SamplingIntervalSeconds * 1000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now}: {ex.Message}");
        }
    }
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[ERROR] Configuration missing: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {DateTime.Now}: {ex.Message}");
}