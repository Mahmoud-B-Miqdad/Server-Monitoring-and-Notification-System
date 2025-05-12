using MongoDB.Driver;
using ServerMonitoringSystem.Shared.Domain;
using ServerMonitoringSystem.Infrastructure.Settings;
using ServerMonitoringSystem.MessageProcessor.Persistence;

namespace ServerMonitoringSystem.Infrastructure.Repositories;

public class MongoDbStatisticsRepository : IStatisticsRepository
{
    private readonly IMongoCollection<ServerStatistics> _statisticsCollection;

    public MongoDbStatisticsRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.MongoDbConnection);
        var database = client.GetDatabase(settings.Database);
        _statisticsCollection = database.GetCollection<ServerStatistics>(settings.Collection);
    }

    public async Task SaveStatisticsAsync(ServerStatistics stats)
    {
        await _statisticsCollection.InsertOneAsync(stats);
    }
}