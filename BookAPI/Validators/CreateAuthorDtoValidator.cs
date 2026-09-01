using BookAPI.DTOs;
using FluentValidation;

namespace BookAPI.Validators
{
	public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
	{
		public CreateAuthorDtoValidator()
		{
			RuleFor(a => a.Name)
				.NotEmpty().WithMessage("Author name is required.")
				.MaximumLength(150).WithMessage("Author name must not exceed 150 characters.");

			RuleFor(a => a.Bio)
				.MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.")
				.When(a => !string.IsNullOrEmpty(a.Bio));
		}
	}
}
