namespace LWSAuthService.Configuration;

public class MongoConfiguration
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string AccountCollectionName { get; set; }
    public string AccessTokenCollectionName { get; set; }
}