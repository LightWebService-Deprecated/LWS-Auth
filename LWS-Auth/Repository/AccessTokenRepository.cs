using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LWS_Auth.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace LWS_Auth.Repository;

public interface IAccessTokenRepository
{
    Task InsertAccessTokenAsync(AccessToken accessToken);
    Task<AccessToken> GetAccessTokenByTokenAsync(string token);
    Task<List<AccessToken>> ListAccessTokensAsync(string userId);
}

public class AccessTokenRepository : IAccessTokenRepository
{
    private readonly IMongoCollection<AccessToken> _accessTokenCollection;

    private IMongoQueryable<AccessToken> AccessTokenQueryable => _accessTokenCollection.AsQueryable();

    public AccessTokenRepository(MongoContext mongoContext)
    {
        _accessTokenCollection = mongoContext.MongoDatabase.GetCollection<AccessToken>(nameof(AccessToken));
        CreateShardAsync(mongoContext, _accessTokenCollection);

        // Create TTL
        var key = Builders<AccessToken>.IndexKeys.Ascending("_ts");
        var indexModel = new CreateIndexModel<AccessToken>(key, new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromMinutes(30)
        });
        _accessTokenCollection.Indexes.CreateOne(indexModel);
    }

    private void CreateShardAsync(MongoContext mongoContext, IMongoCollection<AccessToken> collection)
    {
        var database = mongoContext.MongoDatabase;
        var adminDb = mongoContext.MongoClient.GetDatabase("admin");
        var databaseName = database.DatabaseNamespace.DatabaseName;
        var collectionName = collection.CollectionNamespace.CollectionName;
        adminDb.RunCommand<BsonDocument>(new BsonDocument()
        {
            {"enableSharding", $"{databaseName}"}
        });

        var shardPartition = new BsonDocument
        {
            {"shardCollection", $"{databaseName}.{collectionName}"},
            {"key", new BsonDocument {{"userId", "hashed"}}}
        };
        var command = new BsonDocumentCommand<BsonDocument>(shardPartition);
        var response = adminDb.RunCommand(command);
    }

    public async Task InsertAccessTokenAsync(AccessToken accessToken)
    {
        await _accessTokenCollection.InsertOneAsync(accessToken);
    }

    public async Task<AccessToken> GetAccessTokenByTokenAsync(string token)
    {
        return await AccessTokenQueryable.Where(a => a.Id == token)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AccessToken>> ListAccessTokensAsync(string userId)
    {
        return await AccessTokenQueryable.Where(a => a.UserId == userId)
            .ToListAsync();
    }
}