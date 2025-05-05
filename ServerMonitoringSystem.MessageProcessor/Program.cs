using Microsoft.Extensions.Configuration;
using ServerMonitoringSystem.MessageProcessor.Configuration;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using Microsoft.Extensions.DependencyInjection;
using MessagingLibrary.Interfaces;
using MessagingLibrary.RabbitMq;

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

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<IStatisticsRepository, MongoDbStatisticsRepository>();
services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();

var serviceProvider = services.BuildServiceProvider();

string rabbitMqHost = "localhost";
string exchange = "ServerExchange";
string queue = "ServerStatsQueue";
string routingKey = "ServerStatistics.*";

var processor = new MessageProcessor(serviceProvider.GetRequiredService<IStatisticsRepository>(), rabbitMqHost, exchange, queue, routingKey);
await processor.StartAsync();

Console.WriteLine("Listening to server statistics. Press any key to exit...");
Console.ReadKey();