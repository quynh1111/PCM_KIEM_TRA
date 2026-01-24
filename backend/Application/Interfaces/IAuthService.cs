using System.Threading.Tasks;
using PCM.Application.DTOs.Auth;

namespace PCM.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task RevokeTokenAsync(string userId);
    }
}
