using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using ServerMonitoringSystem.Infrastructure.Repositories;
using ServerMonitoringSystem.Infrastructure.Settings;
using ServerMonitoringSystem.MessageProcessor.Configuration;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using ServerMonitoringSystem.MessageProcessor.Services;
using ServerMonitoringSystem.MessageProcessor.Services.Interfaces;
using ServerMonitoringSystem.Shared.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

try
{
    var anomalyConfig = new AnomalyDetectionConfig
    {
        MemoryUsageAnomalyThresholdPercentage = double.Parse(
            configuration["AnomalyDetection:MemoryAnomalyThreshold"]
            ?? Environment.GetEnvironmentVariable("MEMORY_ANOMALY_THRESHOLD")
            ?? throw new InvalidOperationException("MEMORY_ANOMALY_THRESHOLD not set.")),

        CpuUsageAnomalyThresholdPercentage = double.Parse(
            configuration["AnomalyDetection:CpuAnomalyThreshold"]
            ?? Environment.GetEnvironmentVariable("CPU_ANOMALY_THRESHOLD")
            ?? throw new InvalidOperationException("CPU_ANOMALY_THRESHOLD not set.")),

        MemoryUsageThresholdPercentage = double.Parse(
            configuration["AnomalyDetection:MemoryUsageThreshold"]
            ?? Environment.GetEnvironmentVariable("MEMORY_USAGE_THRESHOLD")
            ?? throw new InvalidOperationException("MEMORY_USAGE_THRESHOLD not set.")),

        CpuUsageThresholdPercentage = double.Parse(
            configuration["AnomalyDetection:CpuUsageThreshold"]
            ?? Environment.GetEnvironmentVariable("CPU_USAGE_THRESHOLD")
            ?? throw new InvalidOperationException("CPU_USAGE_THRESHOLD not set."))
    };

    var signalRConfig = new SignalRConfig
    {
        SignalRUrl = configuration["SignalRConfig:SignalRUrl"]
            ?? Environment.GetEnvironmentVariable("SIGNALR_URL")
            ?? throw new InvalidOperationException("SIGNALR_URL not set.")
    };

    var mongoDbSettings = new MongoDbSettings
    {
        MongoDbConnection = Environment.GetEnvironmentVariable("MONGODB_CONNECTION")
            ?? throw new InvalidOperationException("MONGODB_CONNECTION not set."),

        DatabaseName = configuration["MongoDbSettings:Database"]
            ?? Environment.GetEnvironmentVariable("MONGODB_DATABASE")
            ?? throw new InvalidOperationException("MONGODB_DATABASE not set."),

        CollectionName = configuration["MongoDbSettings:Collection"]
            ?? Environment.GetEnvironmentVariable("MONGODB_COLLECTION")
            ?? throw new InvalidOperationException("MONGODB_COLLECTION not set.")
    };

    var serverConfig = new ServerStatisticsConfig
    {
        ServerIdentifier = configuration["ServerConfig:ServerIdentifier"]
            ?? Environment.GetEnvironmentVariable("SERVER_IDENTIFIER")
            ?? throw new InvalidOperationException("SERVER_IDENTIFIER not set.")
    };

    var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST")
        ?? throw new InvalidOperationException("RABBITMQ_HOST not set.");

    var exchange = configuration["RabbitMQ:Exchange"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE")
        ?? throw new InvalidOperationException("RABBITMQ_EXCHANGE not set.");

    var queue = configuration["RabbitMQ:Queue"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_QUEUE")
        ?? throw new InvalidOperationException("RABBITMQ_QUEUE not set.");

    var routingKey = configuration["RabbitMQ:RoutingKey"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_ROUTING_KEY")
        ?? throw new InvalidOperationException("RABBITMQ_ROUTING_KEY not set.");

    var services = new ServiceCollection();

    services.AddSingleton(anomalyConfig);
    services.AddSingleton(signalRConfig);
    services.AddSingleton(serverConfig);
    services.AddSingleton(mongoDbSettings);
    services.AddScoped<IStatisticsRepository, MongoDbStatisticsRepository>();

    services.AddSingleton<IMessageConsumer>(_ =>
        new RabbitMqConsumer(rabbitMqHost, exchange, queue, routingKey));

    services.AddSingleton<ISignalRAlertSender>(_ =>
        new SignalRAlertSender(signalRConfig.SignalRUrl));

    services.AddSingleton<IAnomalyDetector, AnomalyDetector>();

    services.AddSingleton<IMessageProcessor>(provider =>
    {
        var repo = provider.GetRequiredService<IStatisticsRepository>();
        var detector = provider.GetRequiredService<IAnomalyDetector>();
        var notifier = provider.GetRequiredService<ISignalRAlertSender>();
        var consumer = provider.GetRequiredService<IMessageConsumer>();

        return new MessageProcessor(repo, detector, notifier, consumer, serverConfig);
    });

    var serviceProvider = services.BuildServiceProvider();

    var processor = serviceProvider.GetRequiredService<IMessageProcessor>();
    var started = await processor.StartAsync();

    if (!started)
    {
        Console.WriteLine("Failed to connect to SignalR. Ensure the service is running and try again.");
        return;
    }

    Console.WriteLine("Listening to server statistics. Press Ctrl+C to exit...");
    await Task.Delay(-1);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[CONFIG ERROR] {ex.Message}");
}
catch (FormatException ex)
{
    Console.WriteLine($"[FORMAT ERROR] Invalid numeric format in configuration: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Unexpected error occurred: {ex.Message}");
}
