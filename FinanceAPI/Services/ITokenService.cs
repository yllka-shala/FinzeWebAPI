using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string GenerateRandomToken();
    }
}
