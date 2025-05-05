using Microsoft.AspNetCore.SignalR.Client;

public class SignalRConsumer
{
    private readonly HubConnection _connection;

    public event Action? OnConnected;
    public event Action<int>? OnRetrying;
    public event Action<string, string, DateTime>? OnAlertReceived;

    public SignalRConsumer(string signalRUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(signalRUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartAsync()
    {
        var anomalyAlert = "ReceiveAlert";

        _connection.On<string, string, DateTime>(anomalyAlert, (serverId, alertType, timestamp) =>
        {
            OnAlertReceived?.Invoke(serverId, alertType, timestamp);
        });

        var maxAttempts = 5;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            try
            {
                await _connection.StartAsync();
                OnConnected?.Invoke();
                break;
            }
            catch (HttpRequestException)
            {
                attempt++;
                OnRetrying?.Invoke(attempt);
                await Task.Delay(2000);
            }
        }

        await Task.Delay(-1);
    }


    public async Task StopAsync()
    {
        await _connection.StopAsync();
    }
}
