using System.Text;
using Aviator.Airframe.Config;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aviator.Airframe.Database.Implementation;

public class AcarsMongoDatabase(MongoDbConfig config) : IAcarsDatabase
{
    private readonly MongoClient _client = new(config.ConnectionString);

    private async Task SaveAcarsAsBsonAsync(string jsonString, CancellationToken cancellationToken = default)
    {
        var db = _client.GetDatabase(config.Database);
        var collection = db.GetCollection<BsonDocument>(config.Collection);

        await collection.InsertOneAsync(BsonDocument.Parse(jsonString), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertAsync(byte[] bytes, CancellationToken cancellationToken = default)
    {
        var byteString = Encoding.Default.GetString(bytes);
        await SaveAcarsAsBsonAsync(byteString, cancellationToken).ConfigureAwait(false);
    }
}