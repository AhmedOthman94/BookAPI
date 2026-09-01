using BookAPI.DTOs;
using BookAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers
{
	/// <summary>
	/// Manages authentication endpoints including user registration, login, token refresh, and token revocation.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthController"/> class.
		/// </summary>
		/// <param name="authService">The service handling authentication and JWT token logic.</param>
		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		/// <summary>
		/// Registers a new user in the system.
		/// </summary>
		/// <param name="dto">The registration details containing credentials and user info.</param>
		/// <returns>The generated JWT access token, user details, and sets an HTTP-only refresh token cookie.</returns>
		/// <response code="200">User registered successfully and tokens returned.</response>
		/// <response code="400">If validation fails, passwords do not match, or user already exists.</response>
		[HttpPost("register")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			var response = await _authService.RegisterAsync(dto);
			SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
			return Ok(response);
		}

		/// <summary>
		/// Authenticates a user and issues access and refresh tokens.
		/// </summary>
		/// <param name="dto">The user login credentials.</param>
		/// <returns>The generated JWT access token, user details, and sets an HTTP-only refresh token cookie.</returns>
		/// <response code="200">Authentication successful.</response>
		/// <response code="401">Invalid username/email or password provided.</response>
		[HttpPost("login")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			var response = await _authService.LoginAsync(dto);
			SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
			return Ok(response);
		}

		/// <summary>
		/// Rotates and issues a new access token using the active refresh token stored in request cookies.
		/// </summary>
		/// <returns>A new access and refresh token pair upon successful validation.</returns>
		/// <response code="200">Token pair refreshed successfully.</response>
		/// <response code="400">If the refresh token cookie is missing from the request.</response>
		/// <response code="401">If the refresh token is expired, invalid, or revoked.</response>
		[HttpPost("refresh-token")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> RefreshToken()
		{
			var refreshToken = Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(refreshToken))
				return BadRequest(new { message = "Refresh token is missing from request cookies." });

			var response = await _authService.RefreshTokenAsync(refreshToken);
			SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
			return Ok(response);
		}

		/// <summary>
		/// Revokes an active refresh token provided via payload or request cookies.
		/// </summary>
		/// <param name="dto">Optional payload containing the specific refresh token to revoke.</param>
		/// <returns>A confirmation message indicating successful revocation.</returns>
		/// <response code="200">Token successfully revoked.</response>
		/// <response code="400">If no token is supplied or if the token is already invalid/revoked.</response>
		[HttpPost("revoke-token")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto? dto)
		{
			var token = dto?.Token ?? Request.Cookies["refreshToken"];

			if (string.IsNullOrEmpty(token))
				return BadRequest(new { message = "Token is required." });

			var result = await _authService.RevokeTokenAsync(token);

			if (!result)
				return BadRequest(new { message = "Invalid token or token already revoked." });

			return Ok(new { message = "Token successfully revoked." });
		}

		/// <summary>
		/// Appends an HTTP-only, secure, and same-site restricted refresh token cookie to the HTTP response.
		/// </summary>
		/// <param name="token">The refresh token string.</param>
		/// <param name="expires">The expiration timestamp for the cookie.</param>
		private void SetRefreshTokenInCookie(string token, DateTime expires)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Expires = expires,
				Secure = true,
				SameSite = SameSiteMode.Strict
			};

			Response.Cookies.Append("refreshToken", token, cookieOptions);
		}
	}
}