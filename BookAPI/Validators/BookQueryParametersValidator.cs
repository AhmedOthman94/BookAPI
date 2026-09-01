using BookAPI.Parameters;
using FluentValidation;

namespace BookAPI.Validators
{
	public class BookQueryParametersValidator : AbstractValidator<BookQueryParameters>
	{
		private static readonly string[] AllowedSortColumns = ["title", "price", "stock", "publishedat", "createdat"];

		public BookQueryParametersValidator()
		{
			RuleFor(p => p.PageNumber)
				.GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

			RuleFor(p => p.PageSize)
				.InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

			RuleFor(p => p.SortBy)
				.Must(sortBy => string.IsNullOrEmpty(sortBy) || AllowedSortColumns.Contains(sortBy.Trim().ToLower()))
				.WithMessage($"SortBy can only be one of the following: {string.Join(", ", AllowedSortColumns)}.");

			RuleFor(p => p.PublishedTo)
				.GreaterThanOrEqualTo(p => p.PublishedFrom!.Value)
				.WithMessage("PublishedTo date must be greater than or equal to PublishedFrom date.")
				.When(p => p.PublishedFrom.HasValue && p.PublishedTo.HasValue);
		}
	}
}
