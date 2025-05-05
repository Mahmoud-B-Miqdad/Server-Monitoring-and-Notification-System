using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ServerMonitoringSystem.Shared.Domain;

namespace ServerMonitoringSystem.MessageProcessor.Persistence;

public class MongoDbStatisticsRepository : IStatisticsRepository
{
    private readonly IMongoCollection<ServerStatistics> _statisticsCollection;

    public MongoDbStatisticsRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
        var database = client.GetDatabase("ServerMonitoring");
        _statisticsCollection = database.GetCollection<ServerStatistics>("Statistics");
    }

    public async Task SaveStatisticsAsync(ServerStatistics stats)
    {
        await _statisticsCollection.InsertOneAsync(stats);
    }
}