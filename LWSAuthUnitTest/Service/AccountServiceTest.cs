using System.Collections.Generic;
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

    [Fact(DisplayName =
        "LoginAccount: LoginAccount should return InternalCommunication with result DataNotFound if userEmail is wrong.")]
    public async Task Is_LoginAccount_Returns_InternalCommunication_DataNotFound_If_UserEmail_Is_Wrong()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "testUserEmail@test.com",
            UserPassword = "helloworld"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByEmailAsync(loginRequest.UserEmail))
            .ReturnsAsync(value: null);

        // Do
        var loginResult = await AccountService.LoginAccount(loginRequest);

        // Verify
        _mockAccountRepository.VerifyAll();

        // Check
        Assert.NotNull(loginResult);
        Assert.Equal(ResultType.DataNotFound, loginResult.ResultType);
    }

    [Fact(DisplayName =
        "LoginAccount: LoginAccount should return InternalCommunication with result DataNotFound if userPassword is wrong.")]
    public async Task Is_LoginAccount_Returns_InternalCommunication_DataNotFound_If_UserPassword_Wrong()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "testUserEmail@test.com",
            UserPassword = "helloworld"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByEmailAsync(loginRequest.UserEmail))
            .ReturnsAsync(new Account
            {
                UserEmail = loginRequest.UserEmail,
                UserPassword = "asdl;kasl;dfal;sdjl;asdf;"
            });

        // Do
        var loginResult = await AccountService.LoginAccount(loginRequest);

        // Verify
        _mockAccountRepository.VerifyAll();

        // Check
        Assert.NotNull(loginResult);
        Assert.Equal(ResultType.DataNotFound, loginResult.ResultType);
    }

    [Fact(DisplayName =
        "LoginAccount: LoginAccount should return InternalCommunication with result Success when login succeeds.")]
    public async Task Is_LoginAccount_Works_Well()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "testUserEmail@test.com",
            UserPassword = "helloworld"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByEmailAsync(loginRequest.UserEmail))
            .ReturnsAsync(new Account
            {
                UserEmail = loginRequest.UserEmail,
                UserPassword = BCrypt.Net.BCrypt.HashPassword(loginRequest.UserPassword)
            });

        // Do
        var loginResult = await AccountService.LoginAccount(loginRequest);

        // Verify
        _mockAccountRepository.VerifyAll();

        // Check
        Assert.NotNull(loginResult);
        Assert.Equal(ResultType.Success, loginResult.ResultType);
    }

    [Fact(DisplayName =
        "GetAccountInfoAsync: GetAccountInfoAsync should return InternalCommunication with ResultType NotFound when corresponding data does not exists.")]
    public async Task Is_GetAccountInfoAsync_Returns_NotFound_When_Data_Not_Exists()
    {
        // Let
        var userId = "testUserId";
        
        // Do
        var result = await AccountService.GetAccountInfoAsync(userId);
        
        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultType.DataNotFound, result.ResultType);
    }

    [Fact(DisplayName =
        "GetAccountInfoAsync: GetAccountInfoAsync should return Internal Communication with Result Type Success when data exists")]
    public async Task Is_GetAccountInfoAsync_Returns_Correct_Data_When_Data_Exists()
    {
        // Let
        var userId = "testUserId";
        var testAccount = new Account
        {
            Id = userId,
            UserEmail = "test@test.com",
            UserPassword = "testals;dfkjas;ldkfja",
            AccountRoles = new HashSet<AccountRole> {AccountRole.User},
            UserNickName = "Test"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByIdAsync(userId))
            .ReturnsAsync(testAccount);
        
        // Do
        var result = await AccountService.GetAccountInfoAsync(userId);
        
        // Verify
        _mockAccountRepository.VerifyAll();
        
        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultType.Success, result.ResultType);
        Assert.Equal(testAccount.Id, result.Result.Id);
        Assert.Equal(testAccount.UserEmail, result.Result.UserEmail);
        Assert.Equal(testAccount.UserPassword, result.Result.UserPassword);
        Assert.Equal(testAccount.AccountRoles, result.Result.AccountRoles);
        Assert.Equal(testAccount.UserNickName, result.Result.UserNickName);
    }

    [Fact(DisplayName =
        "RemoveAccountAsync: RemoveAccountAsync should return DataNotFound if corresponding account does not exists.")]
    public async Task Is_RemoveAccountAsync_Returns_DataNotFound_When_Corresponding_Account_Does_Not_Exists()
    {
        // Let
        var userId = "testUserId";
        _mockAccountRepository.Setup(a => a.GetAccountByIdAsync(userId))
            .ReturnsAsync(value: null);
        
        // Do
        var response = await AccountService.RemoveAccountAsync(userId);
        
        // Verify
        _mockAccountRepository.VerifyAll();
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(ResultType.DataNotFound, response.ResultType);
    }

    [Fact(DisplayName =
        "RemoveAccountAsync: RemoveAccountAsync should return internalCommunication with success if removing account succeeds.")]
    public async Task Is_RemoveAccountAsync_Returns_InternalCommunication_Success_When_Remove_Succeeds()
    {
        // Let
        var account = new Account
        {
            Id = "testUserId",
            UserEmail = "test@test.com",
            UserPassword = "testPasasdfasdf",
            UserNickName = "test"
        };
        _mockAccountRepository.Setup(a => a.GetAccountByIdAsync(account.Id))
            .ReturnsAsync(value: account);
        _mockAccountRepository.Setup(a => a.RemoveAccountAsync(It.IsAny<Account>()))
            .Callback((Account inputAccount) =>
            {
                Assert.Equal(account.Id, inputAccount.Id);
                Assert.Equal(account.UserEmail, account.UserEmail);
                Assert.Equal(account.UserPassword, account.UserPassword);
                Assert.Equal(account.UserNickName, account.UserNickName);
            });
        
        // Do
        var response = await AccountService.RemoveAccountAsync(account.Id);
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(ResultType.Success, response.ResultType);
    }
}