using LWSAuthService.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace LWSAuthService.Repository;

public interface IAccountRepository
{
    public Task CreateAccountAsync(Account account);
    public Task<Account?> GetAccountByIdAsync(string userId);
    public Task<Account?> GetAccountByEmailAsync(string userEmail);
    public Task UpdateAccountAsync(Account account);
    public Task RemoveAccountAsync(Account account);
}

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _accountCollection;
    private IMongoQueryable<Account> _accountQueryable => _accountCollection.AsQueryable();

    public AccountRepository(MongoContext mongoContext)
    {
        _accountCollection = mongoContext.AccountCollection;
    }

    public async Task CreateAccountAsync(Account account)
    {
        await _accountCollection.InsertOneAsync(account);
    }

    public async Task<Account?> GetAccountByIdAsync(string userId)
    {
        return await _accountQueryable.Where(a => a.Id == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetAccountByEmailAsync(string userEmail)
    {
        return await _accountQueryable.Where(a => a.UserEmail == userEmail)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAccountAsync(Account account)
    {
        await _accountCollection.ReplaceOneAsync(a => a.Id == account.Id, account, new ReplaceOptions
        {
            IsUpsert = true
        });
    }

    public async Task RemoveAccountAsync(Account account)
    {
        await _accountCollection.DeleteOneAsync(a => a.Id == account.Id);
    }
}