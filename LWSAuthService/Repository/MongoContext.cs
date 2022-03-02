using LWSAuthService.Configuration;
using LWSAuthService.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace LWSAuthService.Repository;

public class MongoContext
{
    public readonly IMongoClient MongoClient;
    public readonly IMongoDatabase MongoDatabase;
    public readonly MongoConfiguration MongoConfiguration;

    public IMongoCollection<AccessToken> AccessTokenCollection =>
        MongoDatabase.GetCollection<AccessToken>(MongoConfiguration.AccessTokenCollectionName);

    public IMongoCollection<Account> AccountCollection =>
        MongoDatabase.GetCollection<Account>(MongoConfiguration.AccountCollectionName);

    public MongoContext(MongoConfiguration mongoConfiguration)
    {
        // Setup MongoDB Naming Convention
        var camelCase = new ConventionPack {new CamelCaseElementNameConvention()};
        ConventionRegistry.Register("CamelCase", camelCase, a => true);

        MongoConfiguration = mongoConfiguration;
        MongoClient = new MongoClient(mongoConfiguration.ConnectionString);
        MongoDatabase = MongoClient.GetDatabase(mongoConfiguration.DatabaseName);

        // Create Indexes
        CreateAccountIndexesAsync().Wait();
        CreateAccessTokenIndexesAsync().Wait();
    }

    private async Task CreateAccessTokenIndexesAsync()
    {
        // TTL
        var timeToLiveKey = Builders<AccessToken>.IndexKeys.Ascending("createdAt");
        var ttlIndexModel = new CreateIndexModel<AccessToken>(timeToLiveKey, new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromMinutes(30)
        });
        await AccessTokenCollection.Indexes.CreateOneAsync(ttlIndexModel);

        // UserId
        var userIdKey = Builders<AccessToken>.IndexKeys.Ascending("userId");
        await AccessTokenCollection.Indexes.CreateOneAsync(new CreateIndexModel<AccessToken>(userIdKey));
    }

    private async Task CreateAccountIndexesAsync()
    {
        // User Email Index
        var userEmailKey = Builders<Account>.IndexKeys.Ascending("userEmail");
        var userEmailIndexModel = new CreateIndexModel<Account>(userEmailKey, new CreateIndexOptions
        {
            Unique = true
        });
        await AccountCollection.Indexes.CreateOneAsync(userEmailIndexModel);
    }
}