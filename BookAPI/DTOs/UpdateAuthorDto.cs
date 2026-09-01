namespace BookAPI.DTOs
{
	public class UpdateAuthorDto
	{
		public required string Name { get; set; }
		public string? Bio { get; set; }
	}
}
