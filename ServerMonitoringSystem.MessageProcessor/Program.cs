using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitoringSystem.MessageProcessor.Configuration;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using ServerMonitoringSystem.MessageProcessor.Services;
using ServerMonitoringSystem.MessageProcessor.Services.Interfaces;
using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;
using ServerMonitoringSystem.Shared.Configuration;

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

var serverConfig = new ServerStatisticsConfig
{
    ServerIdentifier = "Server1" 
};

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(anomalyConfig);
services.AddSingleton(signalRConfig);
services.AddSingleton(serverConfig);

services.AddSingleton<IStatisticsRepository, MongoDbStatisticsRepository>();
services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
services.AddSingleton<ISignalRAlertSender>(provider =>
    new SignalRAlertSender(signalRConfig.SignalRUrl));

services.AddSingleton<IAnomalyDetector, AnomalyDetector>();
services.AddSingleton<IMessageProcessor>(provider =>
{
    var repository = provider.GetRequiredService<IStatisticsRepository>();
    var anomalyDetector = provider.GetRequiredService<IAnomalyDetector>();
    var notifier = provider.GetRequiredService<ISignalRAlertSender>();

    string rabbitMqHost = "localhost";
    string exchange = "ServerExchange";
    string queue = "ServerStatsQueue";
    string routingKey = "ServerStatistics.*";

    return new MessageProcessor(repository, rabbitMqHost, exchange, queue, routingKey, anomalyDetector, notifier);
});

var serviceProvider = services.BuildServiceProvider();

var processor = serviceProvider.GetRequiredService<IMessageProcessor>();
await processor.StartAsync();

Console.WriteLine("Listening to server statistics. Press any key to exit...");
Console.ReadKey();
