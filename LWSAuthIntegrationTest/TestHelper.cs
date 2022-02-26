using System;
using LWS_Auth.Configuration;
using LWS_Auth.Repository;
using LWSAuthIntegrationTest.Helpers;

namespace LWSAuthIntegrationTest;

public class MongoDatabaseHelper
{
    public readonly MongoContext MongoContext;

    protected MongoDatabaseHelper(DockerFixture dockerFixture)
    {
        MongoContext = new MongoContext(new MongoConfiguration
        {
            MongoConnection = dockerFixture.MongoDbContainer.MongoConnection,
            MongoDbName = Guid.NewGuid().ToString()
        });
    }
}