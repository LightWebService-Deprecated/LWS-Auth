using System;
using System.Diagnostics.CodeAnalysis;
using LWS_Auth.Models;

namespace LWS_Auth.Attributes;

[ExcludeFromCodeCoverage]
public class LwsAuthorizeAttribute : Attribute
{
    public AccountRole RequestRole { get; set; }
}