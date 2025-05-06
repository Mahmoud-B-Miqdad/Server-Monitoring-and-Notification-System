namespace SignalREventConsumer;

using Microsoft.AspNetCore.SignalR.Client;

public class SignalRConsumer
{
    private readonly HubConnection _connection;

    public SignalRConsumer(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public void RegisterAlertHandler(Action<string, string, DateTime> handler)
    {
        _connection.On<string, string, DateTime>("ReceiveAlert", (serverId, alertType, timestamp) =>
        {
            handler(serverId, alertType, timestamp);
        });
    }

    public async Task<bool> StartAsync(int maxRetries = 5, int retryDelayMilliseconds = 5000)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _connection.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    throw new Exception($"Failed to connect after {maxRetries} attempts: {ex.Message}", ex);
                }
                else
                {
                    await Task.Delay(retryDelayMilliseconds);
                }
            }
        }

        return false;
    }

    public async Task StopAsync() => await _connection.StopAsync();
}
