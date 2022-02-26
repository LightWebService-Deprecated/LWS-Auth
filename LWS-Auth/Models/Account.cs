using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LWS_Auth.Models;

/// <summary>
/// User model description. All about users!
/// </summary>
[ExcludeFromCodeCoverage]
[BsonIgnoreExtraElements]
public class Account
{
    /// <summary>
    /// Unique ID[Or Identifier] for Each User.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// User's Email Address
    /// </summary>
    [BsonElement("userEmail")]
    public string UserEmail { get; set; }

    /// <summary>
    /// User's Nickname
    /// </summary>
    [BsonElement("userNickName")]
    public string UserNickName { get; set; }

    /// <summary>
    /// User's Password Information. Note this should be encrypted.
    /// </summary>
    [BsonElement("userPassword")]
    public string UserPassword { get; set; }

    [BsonElement("accountRoles")] public HashSet<AccountRole> AccountRoles { get; set; } = new() {AccountRole.User};
}