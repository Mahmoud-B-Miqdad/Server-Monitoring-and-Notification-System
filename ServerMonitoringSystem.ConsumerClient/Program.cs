class Program
{
    static async Task Main(string[] args)
    {
        string signalRUrl = "https://localhost:7271/hub/alerts";
        var consumer = new SignalRConsumer(signalRUrl);

        consumer.OnConnected += () =>
        {
            Console.WriteLine("Connected to SignalR hub.");
        };

        consumer.OnRetrying += (attempt) =>
        {
            Console.WriteLine($"Connection failed. Retrying in 2 seconds... (Attempt {attempt})");
        };

        consumer.OnAlertReceived += (serverId, alertType, timestamp) =>
        {
            Console.WriteLine($"Received alert: {alertType} from {serverId} at {timestamp}");
        };

        await consumer.StartAsync();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        await consumer.StopAsync();
    }
}