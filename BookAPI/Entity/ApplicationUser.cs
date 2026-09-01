using Microsoft.AspNetCore.Identity;

namespace BookAPI.Entity
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public required string FullName { get; set; }
		public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
	}
}
