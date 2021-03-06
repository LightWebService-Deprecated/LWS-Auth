using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LWSAuthService.Models;
using LWSAuthService.Models.Inner;
using LWSAuthService.Models.Request;
using LWSAuthService.Repository;
using LWSAuthService.Service;
using LWSEvent.Event.Account;
using Moq;
using Xunit;

namespace LWSAuthServiceTest.Service;

public class AccountServiceTest
{
    // Dependencies
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IEventRepository> _mockEventRepository;

    // Target object
    private AccountService AccountService =>
        new AccountService(_mockAccountRepository.Object, _mockEventRepository.Object);

    public AccountServiceTest()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
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
        var accountTest = registerRequest.ToUserAccount();
        accountTest.Id = "testId";
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
            })
            .ReturnsAsync(accountTest);
        _mockEventRepository.Setup(a => a.PublishAccountCreated(It.IsAny<AccountCreatedEvent>()))
            .Callback((AccountCreatedEvent message) =>
            {
                Assert.NotNull(message);
                Assert.Equal(accountTest.Id, message.AccountId);
                Assert.NotNull(message.CreatedAt);
            });

        // Do
        var result = await AccountService.CreateNewAccount(registerRequest);

        // Verify
        _mockAccountRepository.VerifyAll();
        _mockEventRepository.VerifyAll();

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
        _mockEventRepository.Setup(a => a.PublishAccountDeleted(It.IsAny<AccountDeletedEvent>()))
            .Callback((AccountDeletedEvent message) =>
            {
                Assert.NotNull(message);

                Assert.Equal(account.Id, message.AccountId);
            });

        // Do
        var response = await AccountService.RemoveAccountAsync(account.Id);

        // Verify
        _mockAccountRepository.VerifyAll();
        _mockEventRepository.VerifyAll();

        // Check
        Assert.NotNull(response);
        Assert.Equal(ResultType.Success, response.ResultType);
    }

    [Fact(DisplayName = "SetNamespaceToken: SetNamespaceToken should d nothing when Account is null.")]
    public async Task Is_SetNamespaceToken_Does_nothing_When_Account_Is_Null()
    {
        // Let
        var targetEvent = new TokenCreatedEvent
        {
            AccountId = Ulid.NewUlid().ToString(),
            NameSpace = $"namespace-test",
            NameSpaceToken = Guid.NewGuid().ToString()
        };
        _mockAccountRepository.Setup(a => a.GetAccountByIdAsync(targetEvent.AccountId))
            .ReturnsAsync(value: null);

        // Do
        await AccountService.SetNamespaceToken(targetEvent);

        // Verify
        _mockAccountRepository.VerifyAll();
    }

    [Fact(DisplayName = "SetNamespaceToken: SetNamespaceToken should update account when event is normal")]
    public async Task Is_SetNamespaceToken_Updates_Account_With_Jwt()
    {
        // Let
        var targetEvent = new TokenCreatedEvent
        {
            AccountId = Ulid.NewUlid().ToString(),
            NameSpace = "test-default",
            NameSpaceToken = Guid.NewGuid().ToString()
        };
        var testAccount = new Account
        {
            Id = targetEvent.AccountId,
            JwtMap = new Dictionary<string, string>(),
            AccountState = AccountState.Created
        };
        _mockAccountRepository.Setup(a => a.GetAccountByIdAsync(testAccount.Id))
            .ReturnsAsync(testAccount);
        _mockAccountRepository.Setup(a => a.UpdateAccountAsync(It.IsAny<Account>()))
            .Callback((Account account) =>
            {
                Assert.Equal(testAccount.Id, account.Id);
                Assert.Single(testAccount.JwtMap);
                Assert.True(testAccount.JwtMap.ContainsKey(targetEvent.NameSpace));
                Assert.Equal(testAccount.JwtMap[targetEvent.NameSpace], targetEvent.NameSpaceToken);
                Assert.Equal(AccountState.Ready, testAccount.AccountState);
            });
        
        // Do
        await AccountService.SetNamespaceToken(targetEvent);
        
        // Verify
        _mockAccountRepository.VerifyAll();
    }
}