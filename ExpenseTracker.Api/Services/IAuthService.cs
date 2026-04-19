namespace ExpenseTracker.Api.Services;

public interface IAuthService
{
    Task<string> Authenticate(string email, string password);
    Task<bool> Register(string email, string userName, string password);
}
