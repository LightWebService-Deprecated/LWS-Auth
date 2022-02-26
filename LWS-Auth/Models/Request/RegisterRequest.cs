using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LWS_Auth.Models.Inner;

namespace LWS_Auth.Models.Request;

[ExcludeFromCodeCoverage]
public class RegisterRequest
{
    public string UserEmail { get; set; }
    public string UserNickName { get; set; }
    public string UserPassword { get; set; }

    public Account ToUserAccount() => new Account
    {
        UserEmail = this.UserEmail,
        UserNickName = this.UserNickName,
        UserPassword = BCrypt.Net.BCrypt.HashPassword(this.UserPassword),
        AccountRoles = new HashSet<AccountRole> {AccountRole.User}
    };

    public InternalCommunication<object> ValidateModel()
    {
        // Validate Email
        var emailRegex = new Regex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
        if (!emailRegex.IsMatch(UserEmail))
        {
            return new InternalCommunication<object>
            {
                Message = "Email address is invalid! Please input correct email address again.",
                ResultType = ResultType.InvalidRequest
            };
        }

        return new InternalCommunication<object>
        {
            ResultType = ResultType.Success
        };
    }
}