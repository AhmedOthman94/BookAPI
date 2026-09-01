namespace BookAPI.Exceptions
{
	public class AuthorNotFoundException : DomainException
	{
		public AuthorNotFoundException(Guid authorId)
			: base($"Author with ID '{authorId}' was not found.", 404) { }
	}
}
