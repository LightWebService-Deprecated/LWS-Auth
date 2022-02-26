using System.Threading.Tasks;
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
}