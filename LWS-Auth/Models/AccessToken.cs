using MongoDB.Bson.Serialization.Attributes;

namespace LWS_Auth.Models;

public class AccessToken
{
    /// <summary>
    /// Unique ID(Hence Token) for each authenticated user.
    /// </summary>
    [BsonId]
    public string Id { get; set; }

    /// <summary>
    /// Shard Key - UserId
    /// </summary>
    public string UserId { get; set; }
}