namespace BookAPI.DTOs
{
	/// <summary>
	/// Data transfer object representing author details sent in API responses.
	/// </summary>
	public class AuthorDto
	{
		/// <summary>
		/// Gets or sets the unique identifier for the author.
		/// </summary>
		/// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the full name of the author.
		/// </summary>
		/// <example>George Orwell</example>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets a brief biographical description of the author.
		/// </summary>
		/// <example>English novelist, essayist, journalist, and critic born in 1903.</example>
		public string? Bio { get; set; }

		/// <summary>
		/// Gets or sets the UTC timestamp when the author record was created.
		/// </summary>
		/// <example>2026-08-15T10:30:00Z</example>
		public DateTime CreatedAt { get; set; }
	}
}