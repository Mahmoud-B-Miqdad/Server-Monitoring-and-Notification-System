using Microsoft.AspNetCore.SignalR.Client;

namespace ServerMonitoringSystem.MessageProcessor.Services
{
    public class SignalRAlertSender : ISignalRAlertSender
    {
        private readonly HubConnection _connection;
        private const string AlertMethodName = "ReceiveAlert";
        private const int SendRetryCount = 3;
        private const int SendRetryDelayMs = 1000;

        public SignalRAlertSender(string signalRUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(signalRUrl)
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task<bool> StartAsync()
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
                catch (Exception ex)
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
            for (int attempt = 1; attempt <= SendRetryCount; attempt++)
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    try
                    {
                        await _connection.InvokeAsync(AlertMethodName, serverId, alertType, timestamp);
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (attempt == SendRetryCount)
                            return;
                        else
                            await Task.Delay(SendRetryDelayMs);
                    }
                }
                else
                {
                    await Task.Delay(SendRetryDelayMs);
                }
            }
        }

        public async Task StopAsync()
        {
                await _connection.StopAsync();
        }
    }
}
