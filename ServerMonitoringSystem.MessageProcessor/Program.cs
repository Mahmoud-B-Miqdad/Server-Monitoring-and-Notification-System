using Microsoft.Extensions.Configuration;
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

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var configuration = builder.Build();

var anomalyConfig = configuration
    .GetSection("AnomalyDetectionConfig")
    .Get<AnomalyDetectionConfig>();

var signalRConfig = configuration
    .GetSection("SignalRConfig")
    .Get<SignalRConfig>();

var mongoDbSettings = configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>();

var serverConfig = new ServerStatisticsConfig
{
    ServerIdentifier = "Server1" 
};

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(anomalyConfig);
services.AddSingleton(signalRConfig);
services.AddSingleton(serverConfig);
services.AddSingleton(mongoDbSettings);
services.AddScoped<IStatisticsRepository, MongoDbStatisticsRepository>();


string rabbitMqHost = "localhost";
string exchange = "ServerExchange";
string queue = "ServerStatsQueue";
string routingKey = "ServerStatistics.*";
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

    return new MessageProcessor(repository, anomalyDetector, notifier, consumer);
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
Console.ReadKey();
