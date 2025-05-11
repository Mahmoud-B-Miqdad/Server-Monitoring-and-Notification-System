using Microsoft.Extensions.Configuration;
using SignalREventConsumer;

string? hubUrl = Environment.GetEnvironmentVariable("SIGNALR_URL");
if (string.IsNullOrEmpty(hubUrl))
{
    Console.WriteLine("SIGNALR_URL environment variable is not set.");
    return;
}

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

await Task.Delay(Timeout.Infinite);
await client.StopAsync();