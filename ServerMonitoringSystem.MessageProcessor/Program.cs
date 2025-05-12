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
    var anomalyConfig = ConfigurationLoader.Load<AnomalyDetectionConfig>(configuration, "AnomalyDetection");
    var signalRConfig = ConfigurationLoader.Load<SignalRConfig>(configuration, "SignalRConfig");
    var mongoDbSettings = ConfigurationLoader.Load<MongoDbSettings>(configuration, "MongoDbSettings");
    var serverConfig = ConfigurationLoader.Load<ServerStatisticsConfig>(configuration, "ServerConfig");
    var rabbitMqConfig = ConfigurationLoader.Load<RabbitMqConfig>(configuration, "RabbitMqConfig");
    var services = new ServiceCollection();

    services.AddSingleton(anomalyConfig);
    services.AddSingleton(signalRConfig);
    services.AddSingleton(serverConfig);
    services.AddSingleton(mongoDbSettings);
    services.AddScoped<IStatisticsRepository, MongoDbStatisticsRepository>();

    services.AddSingleton<IMessageConsumer>(_ =>
        new RabbitMqConsumer(rabbitMqConfig.RabbitHost, rabbitMqConfig.Exchange, rabbitMqConfig.Queue, rabbitMqConfig.RoutingKey));

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
