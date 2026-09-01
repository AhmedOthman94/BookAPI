namespace BookAPI.Exceptions
{
	public class InvalidBookPriceException : DomainException
	{
		public InvalidBookPriceException(decimal price)
			: base($"Book price '{price}' is invalid. Price must be greater than zero.", 400) { }
	}
}
