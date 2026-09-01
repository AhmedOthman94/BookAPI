namespace BookAPI.Exceptions
{
	public class InvalidPublishedDateException : DomainException
	{
		public InvalidPublishedDateException(DateTime publishedAt)
			: base($"Published date '{publishedAt:yyyy-MM-dd}' is invalid.", 400) { }
	}
}
