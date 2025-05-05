using Microsoft.Extensions.Configuration;
using ServerMonitoringSystem.MessageProcessor.Configuration;
using ServerMonitoringSystem.MessageProcessor.Persistence;
using Microsoft.Extensions.DependencyInjection;

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
services.AddSingleton<IStatisticsRepository, MongoDbStatisticsRepository>();

var serviceProvider = services.BuildServiceProvider();