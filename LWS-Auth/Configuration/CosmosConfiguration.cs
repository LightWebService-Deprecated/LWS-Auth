namespace LWS_Auth.Configuration;

public class CosmosConfiguration
{
    public string ConnectionString { get; set; }
    public string CosmosDbname { get; set; }
    public string AccountContainerName { get; set; }
    public string AccessTokenContainerName { get; set; }
}