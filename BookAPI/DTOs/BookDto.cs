namespace BookAPI.DTOs
{
	/// <summary>
	/// Data transfer object representing detailed book information returned in API responses.
	/// </summary>
	public class BookDto
	{
		/// <summary>
		/// Gets or sets the unique identifier for the book.
		/// </summary>
		/// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the main title of the book.
		/// </summary>
		/// <example>1984</example>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the International Standard Book Number (ISBN).
		/// </summary>
		/// <example>978-0451524935</example>
		public string ISBN { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets a brief summary or description of the book's content.
		/// </summary>
		/// <example>A dystopian social science fiction novel and cautionary tale about totalitarianism.</example>
		public string? Description { get; set; }

		/// <summary>
		/// Gets or sets the retail price of the book.
		/// </summary>
		/// <example>14.99</example>
		public decimal Price { get; set; }

		/// <summary>
		/// Gets or sets the current quantity available in stock.
		/// </summary>
		/// <example>150</example>
		public int Stock { get; set; }

		/// <summary>
		/// Gets or sets the unique identifier of the associated author.
		/// </summary>
		/// <example>7e24a180-2a44-42b3-a1d2-0056972621a1</example>
		public Guid AuthorId { get; set; }

		/// <summary>
		/// Gets or sets the display name of the author.
		/// </summary>
		/// <example>George Orwell</example>
		public string AuthorName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the official publication date of the book.
		/// </summary>
		/// <example>1949-06-08T00:00:00Z</example>
		public DateTime PublishedAt { get; set; }

		/// <summary>
		/// Gets or sets the UTC timestamp when the record was created in the system.
		/// </summary>
		/// <example>2026-08-15T10:30:00Z</example>
		public DateTime CreatedAt { get; set; }
	}
}