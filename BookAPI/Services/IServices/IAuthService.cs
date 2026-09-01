using BookAPI.DTOs;

namespace BookAPI.Services.IServices
{
	/// <summary>
	/// Service interface for handling authentication, user registration, token management, and security operations.
	/// </summary>
	public interface IAuthService
	{
		/// <summary>
		/// Registers a new user in the system and generates an initial JWT authentication token pair.
		/// </summary>
		/// <param name="dto">The data transfer object containing user registration details.</param>
		/// <returns>An <see cref="AuthResponseDto"/> containing user credentials, JWT access token, and refresh token.</returns>
		Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

		/// <summary>
		/// Authenticates user credentials and issues a new JWT access token and refresh token.
		/// </summary>
		/// <param name="dto">The data transfer object containing user login credentials (email and password).</param>
		/// <returns>An <see cref="AuthResponseDto"/> containing authenticated user claims and active tokens.</returns>
		Task<AuthResponseDto> LoginAsync(LoginDto dto);

		/// <summary>
		/// Refreshes an expired or active JWT access token using a valid refresh token.
		/// </summary>
		/// <param name="token">The active refresh token string associated with the user session.</param>
		/// <returns>An <see cref="AuthResponseDto"/> containing a new JWT access token and refreshed token data.</returns>
		Task<AuthResponseDto> RefreshTokenAsync(string token);

		/// <summary>
		/// Revokes an active refresh token, effectively logging the user out or invalidating the active session.
		/// </summary>
		/// <param name="token">The refresh token string to be revoked.</param>
		/// <returns><c>true</c> if the token was successfully found and revoked; otherwise, <c>false</c>.</returns>
		Task<bool> RevokeTokenAsync(string token);
	}
}