using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LWS_Auth.Models;

/// <summary>
/// User model description. All about users!
/// </summary>
[ExcludeFromCodeCoverage]
public class Account
{
    /// <summary>
    /// Unique ID[Or Identifier] for Each User.
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// User's Email Address
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    /// User's Nickname
    /// </summary>
    public string UserNickName { get; set; }

    /// <summary>
    /// User's Password Information. Note this should be encrypted.
    /// </summary>
    public string UserPassword { get; set; }

    public HashSet<AccountRole> AccountRoles { get; set; }
}