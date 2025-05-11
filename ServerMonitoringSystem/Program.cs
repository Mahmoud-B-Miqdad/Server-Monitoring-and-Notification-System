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

    var samplingInterval = configuration["ServerConfig:SamplingIntervalSeconds"] ??
                           Environment.GetEnvironmentVariable("SAMPLING_INTERVAL_SECONDS") ??
                           throw new InvalidOperationException("SAMPLING_INTERVAL_SECONDS is not set.");

    var serverIdentifier = configuration["ServerConfig:ServerIdentifier"] ??
                           Environment.GetEnvironmentVariable("SERVER_IDENTIFIER") ??
                           throw new InvalidOperationException("SERVER_IDENTIFIER is not set.");

    var config = new ServerStatisticsConfig
    {
        SamplingIntervalSeconds = int.Parse(samplingInterval),
        ServerIdentifier = serverIdentifier
    };

    var collector = new StatisticsCollector();

    var hostName = configuration["RabbitMqConfig:Host"] ??
                   Environment.GetEnvironmentVariable("RABBITMQ_HOST") ??
                   throw new InvalidOperationException("RABBITMQ_HOST is not set.");
    var exchangeName = configuration["RabbitMqConfig:Exchange"] ??
                       Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ??
                       throw new InvalidOperationException("RABBITMQ_EXCHANGE is not set.");
    var exchangeType = configuration["RabbitMqConfig:ExchangeType"] ??
                       Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE_TYPE") ??
                       throw new InvalidOperationException("RABBITMQ_EXCHANGE_TYPE is not set.");

    var portString = configuration["RabbitMqConfig:Port"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_PORT")
        ?? throw new InvalidOperationException("Missing configuration: RABBITMQ_PORT");

    var Port = int.Parse(portString);

    var userName = configuration["RabbitMqConfig:Username"] ??
                   Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ??
                   throw new InvalidOperationException("Missing configuration: RABBITMQ_USERNAME");

    var password = configuration["RabbitMqConfig:Password"] ??
                   Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ??
                   throw new InvalidOperationException("Missing configuration: RABBITMQ_PASSWORD");

    IMessagePublisher? publisher = null;

    try
    {
        publisher = new RabbitMqPublisher(hostName, exchangeName, exchangeType, Port, userName, password);
    }
    catch (ArgumentNullException ex)
    {
        Console.WriteLine($"Configuration error: {ex.ParamName} - {ex.Message}");
    }

    var monitoringService = new MonitoringService(collector, publisher, config);

    while (true)
    {
        try
        {
            var stats = await monitoringService.RunAsync();

            Console.WriteLine($"[INFO] Published stats: CPU={stats.CpuUsage}%, Memory={stats.MemoryUsage}MB, Available={stats.AvailableMemory}MB");

            await Task.Delay(config.SamplingIntervalSeconds * 1000);
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