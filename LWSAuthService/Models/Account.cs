using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LWSAuthService.Models;

public enum AccountState
{
    Created,
    Ready,
    Revoked,
    DroppedOut,
    Error
}

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
    
    /// <summary>
    /// User Account's State
    /// </summary>
    public AccountState AccountState { get; set; }

    /// <summary>
    /// Namespace : JWT Mapper
    /// </summary>
    public Dictionary<string, string> JwtMap { get; set; } = new();

    public HashSet<AccountRole> AccountRoles { get; set; }
}