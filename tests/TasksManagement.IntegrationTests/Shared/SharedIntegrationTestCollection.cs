namespace TasksManagement.IntegrationTests.Shared;

[CollectionDefinition(nameof(SharedIntegrationTestCollection))]
public class SharedIntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{

}
