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
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole<Guid>> _roleManager;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDBContext _context;

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

		public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
		{
			var user = await _userManager.FindByEmailAsync(dto.UserNameOrEmail) ??
					   await _userManager.FindByNameAsync(dto.UserNameOrEmail);

			if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
				throw new UnauthorizedAccessException("Invalid email/username or password.");

			return await GenerateAuthResponseAsync(user);
		}

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
