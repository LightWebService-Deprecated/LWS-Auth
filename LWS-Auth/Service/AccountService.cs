using System.Threading.Tasks;
using LWS_Auth.Models;
using LWS_Auth.Models.Inner;
using LWS_Auth.Models.Request;
using LWS_Auth.Repository;

namespace LWS_Auth.Service;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<InternalCommunication<object>> CreateNewAccount(RegisterRequest registerRequest)
    {
        var prevAccount = await _accountRepository.GetAccountByEmailAsync(registerRequest.UserEmail);
        if (prevAccount != null)
        {
            return new InternalCommunication<object>
            {
                ResultType = ResultType.DataConflicts,
                Message = $"User email {registerRequest.UserEmail} already registered in server!"
            };
        }

        await _accountRepository.CreateAccountAsync(registerRequest.ToUserAccount());

        return new InternalCommunication<object> {ResultType = ResultType.Success};
    }

    public async Task<InternalCommunication<Account>> LoginAccount(LoginRequest loginRequest)
    {
        var account = await _accountRepository.GetAccountByEmailAsync(loginRequest.UserEmail);
        if (account == null)
        {
            return new InternalCommunication<Account>
            {
                ResultType = ResultType.DataNotFound,
                Message = "Login failed! Please check email or id."
            };
        }

        if (!CheckPasswordCorrect(loginRequest.UserPassword, account.UserPassword))
        {
            return new InternalCommunication<Account>
            {
                ResultType = ResultType.DataNotFound,
                Message = "Login failed! Please check email or id."
            };
        }

        return new InternalCommunication<Account>
        {
            Result = account,
            ResultType = ResultType.Success
        };
    }

    public async Task<InternalCommunication<Account>> GetAccountInfoAsync(string userId)
    {
        var account = await _accountRepository.GetAccountByIdAsync(userId);
        if (account == null)
        {
            return new InternalCommunication<Account>
            {
                ResultType = ResultType.DataNotFound,
                Message = "Cannot find data corresponding userId!"
            };
        }

        return new InternalCommunication<Account>
        {
            ResultType = ResultType.Success,
            Result = account
        };
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
}