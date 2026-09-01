using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookAPI.Data;
using BookAPI.DTOs;
using BookAPI.Entity;
using BookAPI.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookAPI.Services
{
	/// <summary>
	/// Service implementation for managing user authentication, registration, JWT token generation, and refresh token lifecycles.
	/// </summary>
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole<Guid>> _roleManager;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDBContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthService"/> class.
		/// </summary>
		/// <param name="userManager">The ASP.NET Core Identity user manager instance.</param>
		/// <param name="roleManager">The Identity role manager instance for managing user roles.</param>
		/// <param name="configuration">App configuration settings provider for retrieving JWT parameters.</param>
		/// <param name="context">The database context instance for managing entity operations.</param>
		public AuthService(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole<Guid>> roleManager,
			IConfiguration configuration,
			ApplicationDBContext context)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_configuration = configuration;
			_context = context;
		}

		/// <summary>
		/// Registers a new user, assigns default roles, and generates authentication tokens.
		/// </summary>
		/// <param name="dto">The registration data transfer object containing user input details.</param>
		/// <returns>An <see cref="AuthResponseDto"/> containing user information and JWT authentication tokens.</returns>
		/// <exception cref="BadHttpRequestException">Thrown when validation fails, user already exists, or identity creation fails.</exception>
		public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
		{
			if (dto.Password != dto.ConfirmPassword)
				throw new BadHttpRequestException("Password and Confirm Password do not match.");

			var userExists = await _userManager.FindByEmailAsync(dto.Email) ??
							 await _userManager.FindByNameAsync(dto.UserName);
			if (userExists != null)
				throw new BadHttpRequestException("User with this email or username already exists.");

			var user = new ApplicationUser
			{
				Id = Guid.NewGuid(),
				UserName = dto.UserName,
				Email = dto.Email,
				FullName = dto.FullName
			};

			var result = await _userManager.CreateAsync(user, dto.Password);
			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				throw new BadHttpRequestException($"Registration failed: {errors}");
			}

			// Assign default "User" role
			if (!await _roleManager.RoleExistsAsync("User"))
				await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));

			await _userManager.AddToRoleAsync(user, "User");

			return await GenerateAuthResponseAsync(user);
		}

		/// <summary>
		/// Validates user credentials and generates a new access and refresh token pair upon successful authentication.
		/// </summary>
		/// <param name="dto">The login credentials data transfer object.</param>
		/// <returns>An <see cref="AuthResponseDto"/> containing authenticated user details and valid tokens.</returns>
		/// <exception cref="UnauthorizedAccessException">Thrown when user is not found or password check fails.</exception>
		public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
		{
			var user = await _userManager.FindByEmailAsync(dto.UserNameOrEmail) ??
					   await _userManager.FindByNameAsync(dto.UserNameOrEmail);

			if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
				throw new UnauthorizedAccessException("Invalid email/username or password.");

			return await GenerateAuthResponseAsync(user);
		}

		/// <summary>
		/// Validates a refresh token, revokes the current one, and generates a new pair of access and refresh tokens.
		/// </summary>
		/// <param name="token">The existing active refresh token string.</param>
		/// <returns>An <see cref="AuthResponseDto"/> containing newly issued access and refresh tokens.</returns>
		/// <exception cref="UnauthorizedAccessException">Thrown when the refresh token is invalid, expired, or revoked.</exception>
		public async Task<AuthResponseDto> RefreshTokenAsync(string token)
		{
			var user = await _userManager.Users
				.Include(u => u.RefreshTokens)
				.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

			if (user == null)
				throw new UnauthorizedAccessException("Invalid token.");

			var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

			if (!refreshToken.IsActive)
				throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

			// Revoke current refresh token
			refreshToken.RevokedAt = DateTime.UtcNow;

			// Generate new pairs
			return await GenerateAuthResponseAsync(user);
		}

		/// <summary>
		/// Revokes an active refresh token, preventing its future use for refreshing access tokens.
		/// </summary>
		/// <param name="token">The refresh token string to revoke.</param>
		/// <returns><c>true</c> if the token was found and successfully revoked; otherwise, <c>false</c>.</returns>
		public async Task<bool> RevokeTokenAsync(string token)
		{
			var user = await _userManager.Users
				.Include(u => u.RefreshTokens)
				.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

			if (user == null) return false;

			var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

			if (!refreshToken.IsActive) return false;

			refreshToken.RevokedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Generates the complete authentication response object including JWT access token and new refresh token.
		/// </summary>
		/// <param name="user">The application user entity for whom tokens are being generated.</param>
		/// <returns>An <see cref="AuthResponseDto"/> populated with tokens and user details.</returns>
		private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
		{
			var roles = await _userManager.GetRolesAsync(user);

			var (accessToken, accessExpires) = CreateJwtToken(user, roles);
			var refreshToken = CreateRefreshToken(user.Id);

			_context.RefreshTokens.Add(refreshToken);
			await _context.SaveChangesAsync();

			return new AuthResponseDto
			{
				AccessToken = accessToken,
				AccessTokenExpiresAt = accessExpires,
				RefreshToken = refreshToken.Token,
				RefreshTokenExpiresAt = refreshToken.ExpiresAt,
				User = new UserDto
				{
					Id = user.Id,
					UserName = user.UserName!,
					Email = user.Email!,
					FullName = user.FullName,
					Roles = roles
				}
			};
		}

		/// <summary>
		/// Creates a signed JWT access token containing standard identity claims and roles.
		/// </summary>
		/// <param name="user">The user entity whose details are embedded in token claims.</param>
		/// <param name="roles">The list of security roles assigned to the user.</param>
		/// <returns>A tuple containing the serialized JWT string and its expiration timestamp in UTC.</returns>
		private (string Token, DateTime ExpiresAt) CreateJwtToken(ApplicationUser user, IList<string> roles)
		{
			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.UserName!),
				new(ClaimTypes.Email, user.Email!),
				new("fullName", user.FullName)
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenDurationInMinutes"] ?? "15"));

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return (new JwtSecurityTokenHandler().WriteToken(token), expires);
		}

		/// <summary>
		/// Generates a cryptographically secure random refresh token entity for a given user.
		/// </summary>
		/// <param name="userId">The unique identifier of the user owner of the refresh token.</param>
		/// <returns>A new <see cref="RefreshToken"/> instance configured with a 7-day expiration lifespan.</returns>
		private static RefreshToken CreateRefreshToken(Guid userId)
		{
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);

			return new RefreshToken
			{
				Id = Guid.NewGuid(),
				Token = Convert.ToBase64String(randomNumber),
				UserId = userId,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddDays(7)
			};
		}
	}
}