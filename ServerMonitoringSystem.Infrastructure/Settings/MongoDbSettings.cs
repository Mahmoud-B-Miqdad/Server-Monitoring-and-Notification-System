namespace ServerMonitoringSystem.Infrastructure.Settings;

public class MongoDbSettings
{
    public string MongoDbConnection { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
}