using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.MessageProcessor.Persistence;
internal class MongoDbStatisticsRepository : IStatisticsRepository
{
    private readonly IMongoCollection<ServerStatistics> _statisticsCollection;

    public MongoDbStatisticsRepository(IConfiguration configuration)
    {
        var DatabaseConnection = "MongoDbConnection";
        var DatabaseName = "ServerMonitoring";
        var CollectionName = "Statistics";
        var client = new MongoClient(configuration.GetConnectionString(DatabaseConnection));
        var database = client.GetDatabase(DatabaseName);
        _statisticsCollection = database.GetCollection<ServerStatistics>(CollectionName);
    }

    public async Task SaveStatisticsAsync(ServerStatistics stats)
    {
        await _statisticsCollection.InsertOneAsync(stats);
    }
}