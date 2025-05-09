using Microsoft.AspNetCore.SignalR.Client;

namespace ServerMonitoringSystem.MessageProcessor.Services;

public class SignalRAlertSender : ISignalRAlertSender
{
    private const string ReceiveAlertMethod = "ReceiveAlert";
    private readonly HubConnection _connection;

    public SignalRAlertSender(string signalRUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(signalRUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += async (error) =>
        {
            await Task.Delay(3000);
            await TryReconnectAsync();
        };
    }

    public async Task<bool> StartAsync()
    {
        return await TryReconnectAsync();
    }

    private async Task<bool> TryReconnectAsync()
    {
        const int maxRetries = 5;
        const int delayMilliseconds = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _connection.StartAsync();
                return true;
            }
            catch
            {
                if (attempt == maxRetries)
                    return false;
                await Task.Delay(delayMilliseconds);
            }
        }

            return false;
        }

    public async Task SendAlertAsync(string serverId, string alertType, DateTime timestamp)
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(ReceiveAlertMethod, serverId, alertType, timestamp);
        }
    }

    public async Task StopAsync()
    {
        await _connection.StopAsync();
    }
}
