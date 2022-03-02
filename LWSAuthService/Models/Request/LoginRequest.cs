namespace LWSAuthService.Models.Request;

public class LoginRequest
{
    public string UserEmail { get; set; }
    public string UserPassword { get; set; }
}