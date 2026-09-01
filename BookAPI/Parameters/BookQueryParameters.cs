namespace BookAPI.Parameters
{
	public class BookQueryParameters
	{
		public string? SearchTerm { get; set; }
		public Guid? AuthorId { get; set; }
		public DateTime? PublishedFrom { get; set; }
		public DateTime? PublishedTo { get; set; }
		public string? SortBy { get; set; }
		public bool SortDescending { get; set; }
		public int PageNumber { get; set; } = 1; 
		public int PageSize { get; set; } = 10;
	}
}
