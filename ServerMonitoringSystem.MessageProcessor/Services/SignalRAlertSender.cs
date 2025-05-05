using Microsoft.AspNetCore.SignalR.Client;

namespace ServerMonitoringSystem.MessageProcessor.Services;

public class SignalRAlertSender : ISignalRAlertSender
{
    private readonly HubConnection _connection;

    public SignalRAlertSender(string signalRUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(signalRUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartAsync()
    {
        await _connection.StartAsync();
        //Console.WriteLine("SignalR connected");
    }

    public async Task SendAlertAsync(string serverId, string alertType, DateTime timestamp)
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            var anomalyAlert = "ReceiveAlert";
            await _connection.InvokeAsync(anomalyAlert, serverId, alertType, timestamp);
        }
    }

    public async Task StopAsync()
    {
        await _connection.StopAsync();
    }
}