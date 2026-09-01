namespace BookAPI.DTOs
{
	public class CreateAuthorDto
	{
		public required string Name { get; set; }

		public string? Bio { get; set; }
	}
}
