using Newtonsoft.Json;

namespace LWSAuthService.Models;

public class AccessToken
{
    /// <summary>
    /// Unique ID(Hence Token) for each authenticated user.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Shard Key - UserId
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Access Token Roles
    /// </summary>
    public HashSet<AccountRole> Roles { get; set; }

    /// <summary>
    /// Access Token Created At(Epoch)
    /// </summary>
    public long CreatedAt { get; set; }
}