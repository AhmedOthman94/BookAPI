using BookAPI.DTOs;
using FluentValidation;

namespace BookAPI.Validators
{
	public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
	{
		public CreateBookDtoValidator()
		{
			RuleFor(b => b.Title)
				.NotEmpty().WithMessage("Book title is required.")
				.MaximumLength(200).WithMessage("Book title must not exceed 200 characters.");

			RuleFor(b => b.ISBN)
				.NotEmpty().WithMessage("ISBN is required.")
				.MaximumLength(17).WithMessage("ISBN must not exceed 17 characters.")
				.Matches(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$")
				.WithMessage("Invalid ISBN format. Expected ISBN-10 or ISBN-13 format.");

			RuleFor(b => b.Description)
				.MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
				.When(b => !string.IsNullOrEmpty(b.Description));

			RuleFor(b => b.Price)
				.GreaterThan(0).WithMessage("Price must be greater than 0.");

			RuleFor(b => b.Stock)
				.GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

			RuleFor(b => b.AuthorId)
				.NotEmpty().WithMessage("AuthorId is required.");

			RuleFor(b => b.PublishedAt)
				.NotEmpty().WithMessage("Published date is required.")
				.LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.");
		}
	}
}
