using Fundo.Applications.WebApi.DTOs;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDto loginDto);
    }
}
