using Fundo.Applications.WebApi.DTOs;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDto loginDto);
        Task<bool> ValidateUserAsync(string username, string password);
        string HashPassword(string password);
        string GenerateRefreshToken();
    }
}
