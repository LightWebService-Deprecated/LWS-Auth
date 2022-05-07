using System;
using LWSAuthService.Configuration;
using LWSAuthService.Repository;
using MongoDB.Driver;
using Xunit;

namespace LWSAuthServiceTest.Helpers;

[CollectionDefinition("IntegrationCollections")]
public class IntegrationTestDefinition: ICollectionFixture<IntegrationTestFixture>
{
}

public class IntegrationTestFixture
{
    private readonly MongoClient _MongoClient;
    public MongoContext _MongoContext => new(TestMongoConfiguration);

    public MongoConfiguration TestMongoConfiguration => new MongoConfiguration
    {
        ConnectionString = ConnectionString,
        DatabaseName = "IntegrationTestDatabase",
        AccountCollectionName = Ulid.NewUlid().ToString(),
        AccessTokenCollectionName = Ulid.NewUlid().ToString()
    };

    private const string ConnectionString = "mongodb://root:testPassword@localhost:27017";
    
    public IntegrationTestFixture()
    {
        _MongoClient = new MongoClient(ConnectionString);
    }
}