using System.Linq;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using LWS_Auth.Extension;
using LWS_Auth.Models;
using Microsoft.Azure.Cosmos;

namespace LWS_Auth.Repository;

public interface IAccountRepository
{
    public Task CreateAccountAsync(Account account);
    public Task<Account> GetAccountByIdAsync(string userId);
    public Task<Account> GetAccountByEmailAsync(string userEmail);
    public Task UpdateAccountAsync(Account account);
    public Task RemoveAccountAsync(Account account);
}

public class AccountRepository : IAccountRepository
{
    private readonly Container _accountContainer;
    private IQueryable<Account> AccountQueryable => _accountContainer.GetItemLinqQueryable<Account>();

    public AccountRepository(CosmosClient cosmosClient, CosmosConfiguration cosmosConfiguration)
    {
        _accountContainer =
            cosmosClient.GetContainer(cosmosConfiguration.CosmosDbname, cosmosConfiguration.AccountContainerName);
    }

    public async Task CreateAccountAsync(Account account)
    {
        await _accountContainer.CreateItemAsync(account, new PartitionKey(account.UserEmail));
    }

    public async Task<Account> GetAccountByIdAsync(string userId)
    {
        return await AccountQueryable.Where(a => a.Id == userId)
            .CosmosFirstOrDefaultAsync();
    }

    public async Task<Account> GetAccountByEmailAsync(string userEmail)
    {
        return await AccountQueryable.Where(a => a.UserEmail == userEmail)
            .CosmosFirstOrDefaultAsync();
    }

    public async Task UpdateAccountAsync(Account account)
    {
        await _accountContainer.UpsertItemAsync(account);
    }

    public async Task RemoveAccountAsync(Account account)
    {
        await _accountContainer.DeleteItemAsync<Account>(account.Id, new PartitionKey(account.UserEmail));
    }
}