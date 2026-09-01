namespace BookAPI.DTOs
{
	public class UserDto
	{
		public Guid Id { get; set; }

		public string UserName { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string FullName { get; set; } = string.Empty;

		public IList<string> Roles { get; set; } = [];
	}
}
