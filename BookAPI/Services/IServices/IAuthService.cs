using BookAPI.DTOs;

namespace BookAPI.Services.IServices
{
	public interface IAuthService
	{
		Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
		Task<AuthResponseDto> LoginAsync(LoginDto dto);
		Task<AuthResponseDto> RefreshTokenAsync(string token);
		Task<bool> RevokeTokenAsync(string token);
	}
}
