namespace BookAPI.DTOs
{
	public class AuthorDto
	{
		public Guid Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public string? Bio { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
