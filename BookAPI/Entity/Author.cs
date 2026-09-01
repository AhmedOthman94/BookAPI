namespace BookAPI.Entity
{
	public class Author
	{
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public string? Bio {  get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public ICollection<Book> Books { get; set; } = new List<Book>();
	}
}
