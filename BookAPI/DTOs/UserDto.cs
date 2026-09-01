namespace BookAPI.DTOs
{
	/// <summary>
	/// Data transfer object representing user identity and role details returned in authentication and user management responses.
	/// </summary>
	public class UserDto
	{
		/// <summary>
		/// Gets or sets the unique identifier for the user.
		/// </summary>
		/// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the unique username used for logging into the application.
		/// </summary>
		/// <example>johndoe</example>
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the primary email address associated with the user account.
		/// </summary>
		/// <example>john.doe@example.com</example>
		public string Email { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the full legal or display name of the user.
		/// </summary>
		/// <example>John Doe</example>
		public string FullName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the list of authorization roles assigned to the user.
		/// </summary>
		/// <example>["Admin", "User"]</example>
		public IList<string> Roles { get; set; } = [];
	}
}