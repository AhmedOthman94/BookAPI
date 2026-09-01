using BookAPI.DTOs;
using BookAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			var response = await _authService.RegisterAsync(dto);
			SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
			return Ok(response);
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			var response = await _authService.LoginAsync(dto);
			SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
			return Ok(response);
		}

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