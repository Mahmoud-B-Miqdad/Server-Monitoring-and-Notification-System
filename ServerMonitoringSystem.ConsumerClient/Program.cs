using Microsoft.Extensions.Configuration;
using SignalREventConsumer;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

string hubUrl = configuration["SignalRConfig:SignalRUrl"];

var client = new SignalRConsumer(hubUrl);

client.RegisterAlertHandler((serverId, alertType, timestamp) =>
{
    Console.WriteLine($"[Alert] Server: {serverId}, Type: {alertType}, Time: {timestamp}");
});

Console.WriteLine("Connecting to SignalR hub...");

bool connected = await client.StartAsync();

if (!connected)
{
    Console.WriteLine("Failed to connect after multiple attempts. Make sure the SignalR server is running.");
    return;
}

Console.WriteLine("Connected to SignalR hub. Listening for alerts...");
Console.WriteLine("Press any key to exit.");

Console.ReadKey();
await client.StopAsync();