using System;
using LWS_Auth.Models;

namespace LWS_Auth.Attributes;

public class LwsAuthorizeAttribute : Attribute
{
    public AccountRole RequestRole { get; set; }
}