using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookAPI.Entity
{
	public class Book
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string ISBN { get; set; } = string.Empty;
		public Guid AuthorId { get; set; }
		public Author Author { get; set; } = null!;
		public string Description { get; set; } = string.Empty;
		public DateTime PublishedAt { get; set; }
		public int Stock {  get; set; }
		public decimal Price { get; set; }
		public DateTime CraetedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdateAt { get; set; }
	}
}
