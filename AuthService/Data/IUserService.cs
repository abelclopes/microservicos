using AuthService.Entity;

namespace AuthService.Data;
public interface IUserService
{
    void Register(User user);
    TokenResponse Login(User user);
}