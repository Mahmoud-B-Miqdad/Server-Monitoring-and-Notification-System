using ServerMonitoring.SignalRHub.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.UseRouting();

app.MapHub<AlertHub>("/hub/alerts");

app.Run();