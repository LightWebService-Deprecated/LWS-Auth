using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LWS_Auth.Models;
using LWS_Auth.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace LWSAuthIntegrationTest.Repository;

public class AccountRepositoryTest : MongoDatabaseHelper
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMongoCollection<Account> _accountCollection;

    private Account TestAccount => new Account
    {
        UserEmail = "kangdroid@test.com",
        UserNickName = "testUserNickName",
        UserPassword = "helloworld",
        AccountRoles = new HashSet<AccountRole> {AccountRole.Admin}
    };

    public AccountRepositoryTest()
    {
        _accountRepository = new AccountRepository(MongoContext);
        _accountCollection = MongoContext.MongoDatabase.GetCollection<Account>(nameof(Account));
    }

    private async Task<List<Account>> GetAccountCollectionAsync()
    {
        var result = await _accountCollection.FindAsync(FilterDefinition<Account>.Empty);
        return await result.ToListAsync();
    }

    private void AccountEqual(Account testAccount, Account expectedAccount)
    {
        // Make sure saved entities are the one we tried to save.
        Assert.Equal(testAccount.UserEmail, expectedAccount.UserEmail);
        Assert.Equal(testAccount.UserNickName, expectedAccount.UserNickName);
        Assert.Equal(testAccount.UserPassword, expectedAccount.UserPassword);
        Assert.Equal(testAccount.AccountRoles.Count, expectedAccount.AccountRoles.Count);
        Assert.Equal(testAccount.AccountRoles.First(), expectedAccount.AccountRoles.First());
    }

    [Fact(DisplayName = "CreateAccountAsync: CreateAccountAsync should create account entity to database.")]
    public async Task Is_CreateAccountAsync_Creates_Single_AccountEntity()
    {
        // Let
        var testAccount = TestAccount;

        // Do
        await _accountRepository.CreateAccountAsync(testAccount);

        // Make sure we confirm single entity.
        var accountList = await GetAccountCollectionAsync();
        Assert.Single(accountList);

        // Make sure saved entities are the one we tried to save.
        var expectedAccount = accountList[0];
        AccountEqual(testAccount, expectedAccount);
    }

    [Fact(DisplayName = "GetAccountByIdAsync: GetAccountByIdAsync should return account entity when data exists.")]
    public async Task Is_GetAccountByIdAsync_Returns_Account_Entity_When_Data_Exists()
    {
        // let
        var testAccount = TestAccount;
        await _accountCollection.InsertOneAsync(testAccount);

        // Do
        var foundAccount = await _accountRepository.GetAccountByIdAsync(testAccount.Id);

        // Make sure we have the account we need.
        Assert.NotNull(foundAccount);
        AccountEqual(testAccount, foundAccount);
    }

    [Fact(DisplayName =
        "GetAccountByIdAsync: GetAccountByIdAsync should return null when there is no corresponding data.")]
    public async Task Is_GetAccountByIdAsync_Returns_Null_When_No_Data()
    {
        // do
        var foundAccount = await _accountRepository.GetAccountByIdAsync(ObjectId.GenerateNewId().ToString());

        // Check
        Assert.Null(foundAccount);
    }

    [Fact(DisplayName =
        "GetAccountByEmailAsync: GetAccountByEmailAsync should return account entity when corresponding account entity for email exists.")]
    public async Task Is_GetAccountByIdAsync_Returns_AccountEntity_When_Data_Exists()
    {
        // Let
        var testAccount = TestAccount;
        await _accountCollection.InsertOneAsync(testAccount);

        // Do
        var response = await _accountRepository.GetAccountByEmailAsync(testAccount.UserEmail);

        // Check
        Assert.NotNull(response);
        AccountEqual(testAccount, response);
    }

    [Fact(DisplayName =
        "GetAccountByEmailAsync: GetAccountByEmailAsync should return null when corresponding account is not found.")]
    public async Task Is_GetAccountByEmailAsync_Returns_Null_When_Data_Not_Found()
    {
        // do
        var foundAccount = await _accountRepository.GetAccountByEmailAsync(TestAccount.UserEmail);

        // Check
        Assert.Null(foundAccount);
    }

    [Fact(DisplayName = "UpdateAccountAsync: UpdateAccountAsync should update document well.")]
    public async Task Is_UpdateAccountAsync_Update_Document_Well()
    {
        // Let
        var testAccount = TestAccount;
        await _accountCollection.InsertOneAsync(testAccount);

        // Do
        testAccount.UserPassword = "AnotherTestPassword";
        await _accountRepository.UpdateAccountAsync(testAccount);

        // Check
        var dataList = await GetAccountCollectionAsync();
        Assert.Single(dataList);

        // Make sure saved entities are the one we tried to save.
        var expectedAccount = dataList[0];
        Assert.Equal(testAccount.UserEmail, expectedAccount.UserEmail);
        Assert.Equal(testAccount.UserNickName, expectedAccount.UserNickName);
        Assert.Equal(testAccount.UserPassword, expectedAccount.UserPassword);
        Assert.Equal(testAccount.AccountRoles.Count, expectedAccount.AccountRoles.Count);
        Assert.Equal(testAccount.AccountRoles.First(), expectedAccount.AccountRoles.First());
    }

    [Fact(DisplayName = "RemoveAccountAsync: RemoveAccountAsync should remove account information well.")]
    public async Task Is_RemoveAccountAsync_Removes_Account_Well()
    {
        // Let
        var testAccount = TestAccount;
        await _accountCollection.InsertOneAsync(testAccount);

        // Do
        await _accountRepository.RemoveAccountAsync(testAccount.Id);

        // Check
        var dataList = await GetAccountCollectionAsync();
        Assert.Empty(dataList);
    }
}