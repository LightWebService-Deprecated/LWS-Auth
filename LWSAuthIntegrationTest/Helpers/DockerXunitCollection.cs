using Xunit;

namespace LWSAuthIntegrationTest.Helpers;

[CollectionDefinition("DockerIntegration")]
public class DockerXunitCollection : ICollectionFixture<DockerFixture>
{
}