using LWS_Auth.Models.Request;
using Xunit;

namespace LWSEndToEndTest.TestData;

public class AccountLoginFailTestSet
{
    public LoginRequest LoginRequest { get; set; }
    public bool Register { get; set; }
}

public static class AccountTriggerData
{
    public static TheoryData<AccountLoginFailTestSet> LoginFailData => new()
    {
        // User Email is not found
        new AccountLoginFailTestSet
        {
            LoginRequest = new LoginRequest
            {
                UserEmail = "test@test.com",
                UserPassword = "helloworld"
            },
            Register = false
        },

        // User email found - but password does not match
        new AccountLoginFailTestSet
        {
            LoginRequest = new LoginRequest
            {
                UserEmail = "test@test.com",
                UserPassword = "helloworld"
            },
            Register = true
        }
    };
}