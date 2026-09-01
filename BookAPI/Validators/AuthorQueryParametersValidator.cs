using BookAPI.Parameters;
using FluentValidation;

namespace BookAPI.Validators
{
	public class AuthorQueryParametersValidator : AbstractValidator<AuthorQueryParameters>
	{
		private static readonly string[] AllowedSortColumns = ["name", "createdat"];

		public AuthorQueryParametersValidator()
		{
			RuleFor(p => p.PageNumber)
				.GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

			RuleFor(p => p.PageSize)
				.InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

			RuleFor(p => p.SortBy)
				.Must(sortBy => string.IsNullOrEmpty(sortBy) || AllowedSortColumns.Contains(sortBy.Trim().ToLower()))
				.WithMessage($"SortBy can only be one of the following: {string.Join(", ", AllowedSortColumns)}.");
		}
	}
}
