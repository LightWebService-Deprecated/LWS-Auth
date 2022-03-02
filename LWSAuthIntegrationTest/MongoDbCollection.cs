using Xunit;

namespace LWSAuthIntegrationTest;

[CollectionDefinition("MongoDb")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
}