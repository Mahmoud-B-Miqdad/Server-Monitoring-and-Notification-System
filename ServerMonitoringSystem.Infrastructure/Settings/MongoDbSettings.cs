namespace ServerMonitoringSystem.Infrastructure.Settings;

public class MongoDbSettings
{
    public string MongoDbConnection { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
}