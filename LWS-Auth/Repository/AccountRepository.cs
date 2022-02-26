using System.Threading.Tasks;
using LWS_Auth.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace LWS_Auth.Repository;

public interface IAccountRepository
{
    public Task CreateAccountAsync(Account account);
    public Task<Account> GetAccountByIdAsync(string userId);
    public Task<Account> GetAccountByEmailAsync(string userEmail);
    public Task UpdateAccountAsync(Account account);
    public Task RemoveAccountAsync(string userId);
}

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _accountCollection;
    private IMongoQueryable<Account> AccountQueryable => _accountCollection.AsQueryable();

    public AccountRepository(MongoContext mongoContext)
    {
        _accountCollection = mongoContext.MongoDatabase.GetCollection<Account>(nameof(Account));
    }

    public async Task CreateAccountAsync(Account account)
    {
        await _accountCollection.InsertOneAsync(account);
    }

    public async Task<Account> GetAccountByIdAsync(string userId)
    {
        return await AccountQueryable.Where(a => a.Id == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<Account> GetAccountByEmailAsync(string userEmail)
    {
        return await AccountQueryable.Where(a => a.UserEmail == userEmail)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAccountAsync(Account account)
    {
        await _accountCollection.ReplaceOneAsync(a => a.Id == account.Id, account,
            new ReplaceOptions {IsUpsert = true});
    }

    public async Task RemoveAccountAsync(string userId)
    {
        await _accountCollection.DeleteOneAsync(a => a.Id == userId);
    }
}