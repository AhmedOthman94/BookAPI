namespace BookAPI.Exceptions
{
	public class InvalidBookStockException : DomainException
	{
		public InvalidBookStockException(int stock)
			: base($"Book stock '{stock}' is invalid. Stock cannot be negative.", 400) { }
	}
}
