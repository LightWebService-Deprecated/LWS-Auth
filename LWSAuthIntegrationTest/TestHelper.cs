using System;
using LWS_Auth.Configuration;
using LWS_Auth.Repository;

namespace LWSAuthIntegrationTest;

public class MongoDatabaseHelper : IDisposable
{
    public readonly MongoContext MongoContext;
    private readonly MongoConfiguration _mongoConfiguration;

    protected MongoDatabaseHelper()
    {
        var cosmosConnectionString = Environment.GetEnvironmentVariable("E2E_MONGODB_CONNECTION")
                                     ?? throw new NullReferenceException("Mongodb Connection is not defined!");
        _mongoConfiguration = new MongoConfiguration
        {
            MongoConnection = cosmosConnectionString,
            MongoDbName = Guid.NewGuid().ToString()
        };

        MongoContext = new MongoContext(_mongoConfiguration);
    }

    public void Dispose()
    {
        MongoContext.MongoClient.DropDatabase(_mongoConfiguration.MongoDbName);
    }
}