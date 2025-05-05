using Microsoft.Extensions.Configuration;
using ServerMonitoringSystem.MessageProcessor.Configuration;

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