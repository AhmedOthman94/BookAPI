namespace BookAPI.Exceptions
{
	public class BookNotFoundException : DomainException
	{
		public BookNotFoundException(Guid bookId)
			: base($"Book with ID '{bookId}' was not found.", 404) { }
	}
}
