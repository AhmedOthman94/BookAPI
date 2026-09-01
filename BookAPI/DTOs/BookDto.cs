namespace BookAPI.DTOs
{
	public class BookDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = string.Empty;
		public string ISBN { get; set; } = string.Empty;

		public string? Description { get; set; }

		public decimal Price { get; set; }

		public int Stock { get; set; }

		public Guid AuthorId { get; set; }

		public string AuthorName { get; set; } = string.Empty;
		public DateTime PublishedDate { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
