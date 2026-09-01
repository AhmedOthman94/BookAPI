namespace BookAPI.Exceptions
{
	public class DuplicateBookException : DomainException
	{
		public DuplicateBookException(string title)
			: base($"A book with the title '{title}' already exists.", 409) { }
	}
}
