namespace BookAPI.DTOs
{
	public class UpdateBookDto
	{
		public required string Title { get; set; }
		public required string ISBN { get; set; }

		public string? Description { get; set; }

		public decimal Price { get; set; }

		public int Stock { get; set; }

		public Guid AuthorId { get; set; }
		public DateTime PublishedAt { get; set; }
	}
}
