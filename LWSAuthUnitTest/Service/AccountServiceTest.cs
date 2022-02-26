using System.Linq;
using System.Threading.Tasks;
using LWS_Auth.Models;
using LWS_Auth.Models.Inner;
using LWS_Auth.Models.Request;
using LWS_Auth.Repository;
using LWS_Auth.Service;
using Moq;
using Xunit;

namespace LWSAuthUnitTest.Service;

public class AccountServiceTest
{
    // Dependencies
    private readonly Mock<IAccountRepository> _mockAccountRepository;

    // Target object
    private AccountService AccountService => new AccountService(_mockAccountRepository.Object);

    public AccountServiceTest()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
    }

    private bool CheckPasswordCorrect(string plainPassword, string hashedPassword)
    {
        bool correct = false;
        try
        {
            correct = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
        catch
        {
        }

        return correct;
    }

    [Fact(DisplayName =
        "CreateNewAccount: CreateNewAccount should return 'DataConflicts' ResultType when account with same userEmail exists.")]
    public async Task Is_CreateNewAccount_Returns_DataConflicts_When_Account_With_Same_UserEmail_Exists()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "testUserEMail@test.com",
            UserPassword = "testPassword",
            UserNickName = "testNickName"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByEmailAsync(registerRequest.UserEmail))
            .ReturnsAsync(new Account());

        // Do
        var result = await AccountService.CreateNewAccount(registerRequest);

        // Verify
        _mockAccountRepository.VerifyAll();

        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultType.DataConflicts, result.ResultType);
    }

    [Fact(DisplayName =
        "CreateNewAccount: CreateNewAccount should return 'Success' Result Type register request is completely new-user.")]
    public async Task Is_CreateNewAccount_Should_Return_Success_When_Completely_New_User()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "testUserEMail@test.com",
            UserPassword = "testPassword",
            UserNickName = "testNickName"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByEmailAsync(registerRequest.UserEmail))
            .ReturnsAsync(value: null);
        _mockAccountRepository.Setup(a => a.CreateAccountAsync(It.IsAny<Account>()))
            .Callback((Account account) =>
            {
                Assert.Equal(registerRequest.UserEmail, account.UserEmail);
                Assert.Equal(registerRequest.UserNickName, account.UserNickName);
                Assert.True(CheckPasswordCorrect(registerRequest.UserPassword, account.UserPassword));
                Assert.Single(account.AccountRoles);
                Assert.Equal(AccountRole.User, account.AccountRoles.First());
            });

        // Do
        var result = await AccountService.CreateNewAccount(registerRequest);

        // Verify
        _mockAccountRepository.VerifyAll();

        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultType.Success, result.ResultType);
    }
}