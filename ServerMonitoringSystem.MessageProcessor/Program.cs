using Microsoft.Extensions.DependencyInjection;
using ServerMonitoringSystem.MessageProcessor.Configuration;
using ServerMonitoringSystem.MessageProcessor.Services;
using ServerMonitoringSystem.MessageProcessor.Services.Interfaces;
using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using ServerMonitoringSystem.Shared.Configuration;
using ServerMonitoringSystem.Infrastructure.Settings;
using ServerMonitoringSystem.Infrastructure.Repositories;
using ServerMonitoringSystem.MessageProcessor.Persistence;

var anomalyConfig = new AnomalyDetectionConfig
{
    MemoryUsageAnomalyThresholdPercentage = double.Parse(Environment.GetEnvironmentVariable("MEMORY_ANOMALY_THRESHOLD") ?? "0.4"),
    CpuUsageAnomalyThresholdPercentage = double.Parse(Environment.GetEnvironmentVariable("CPU_ANOMALY_THRESHOLD") ?? "0.5"),
    MemoryUsageThresholdPercentage = double.Parse(Environment.GetEnvironmentVariable("MEMORY_USAGE_THRESHOLD") ?? "0.8"),
    CpuUsageThresholdPercentage = double.Parse(Environment.GetEnvironmentVariable("CPU_USAGE_THRESHOLD") ?? "0.9")
};

var signalRConfig = new SignalRConfig
{
    SignalRUrl = Environment.GetEnvironmentVariable("SIGNALR_URL") ?? "https://localhost:7271/hub/alerts"
};

MongoDbSettings mongoDbSettings;

try
{
    mongoDbSettings = new MongoDbSettings
    {
        MongoDbConnection = Environment.GetEnvironmentVariable("MONGODB_CONNECTION")
                            ?? throw new InvalidOperationException("MONGODB_CONNECTION is not set."),
        DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE")
                            ?? throw new InvalidOperationException("MONGODB_DATABASE is not set."),
        CollectionName = Environment.GetEnvironmentVariable("MONGODB_COLLECTION")
                            ?? throw new InvalidOperationException("MONGODB_COLLECTION is not set.")
    };
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing MongoDbSettings: {ex.Message}");
    throw; 
}


var serverConfig = new ServerStatisticsConfig
{
    ServerIdentifier = Environment.GetEnvironmentVariable("SERVER_IDENTIFIER") ?? "Server1"
};

var services = new ServiceCollection();
services.AddSingleton(anomalyConfig);
services.AddSingleton(signalRConfig);
services.AddSingleton(serverConfig);
services.AddSingleton(mongoDbSettings);
services.AddScoped<IStatisticsRepository, MongoDbStatisticsRepository>();


string rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
string exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "ServerExchange";
string queue = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "ServerStatsQueue";
string routingKey = Environment.GetEnvironmentVariable("RABBITMQ_ROUTING_KEY") ?? "ServerStatistics.*";

services.AddSingleton<IMessageConsumer>(provider =>
{
    return new RabbitMqConsumer(
        hostname: rabbitMqHost,
        exchange: exchange,
        queue: queue,
        routingKey: routingKey
    );
});

services.AddSingleton<ISignalRAlertSender>(provider =>
    new SignalRAlertSender(signalRConfig.SignalRUrl));

services.AddSingleton<IAnomalyDetector, AnomalyDetector>();
services.AddSingleton<IMessageProcessor>(provider =>
{
    var repository = provider.GetRequiredService<IStatisticsRepository>();
    var anomalyDetector = provider.GetRequiredService<IAnomalyDetector>();
    var notifier = provider.GetRequiredService<ISignalRAlertSender>();
    var consumer = provider.GetRequiredService<IMessageConsumer>();

    return new MessageProcessor(repository, anomalyDetector, notifier, consumer, serverConfig);
});

var serviceProvider = services.BuildServiceProvider();

var processor = serviceProvider.GetRequiredService<IMessageProcessor>();
var startedSuccessfully = await processor.StartAsync();

if (!startedSuccessfully)
{
    Console.WriteLine("Failed to connect to the SignalR server. Make sure the service is running and try again.");
    return;
}

Console.WriteLine("Listening to server statistics. Press any key to exit...");
await Task.Delay(-1);
