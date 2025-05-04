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

await monitoringService.RunAsync();
