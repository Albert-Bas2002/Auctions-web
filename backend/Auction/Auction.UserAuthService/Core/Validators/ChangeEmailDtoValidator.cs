using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using FluentValidation;
namespace Auction.UserAuthService.Core.Validators
{
    public class ChangeEmailDtoValidator : AbstractValidator<ChangeEmailDto>
    {
        public ChangeEmailDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("Current username is required.")
                .MaximumLength(User.EMAIL_MAX_LENGTH).WithMessage($"Current username must be at most {User.EMAIL_MAX_LENGTH} characters.")
                .EmailAddress().WithMessage("Please enter a valid email address");

        }
    }
}
