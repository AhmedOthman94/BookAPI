namespace BookAPI.Entity
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public string Token { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? RevokedAt { get; set; } // Foreign Key
		public Guid UserId { get; set; } // Navigation Property
		public ApplicationUser User { get; set; } = null!; 
		public bool IsExpired => DateTime.UtcNow >= ExpiresAt; 
		public bool IsRevoked => RevokedAt.HasValue; 
		public bool IsActive => !IsExpired && !IsRevoked;
	}
}
