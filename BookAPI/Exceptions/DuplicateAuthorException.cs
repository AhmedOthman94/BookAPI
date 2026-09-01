namespace BookAPI.Exceptions
{
	public class DuplicateAuthorException : DomainException
	{
		public DuplicateAuthorException(string authorName)
			: base($"An author with the name '{authorName}' already exists.", 409) { }
	}
}
