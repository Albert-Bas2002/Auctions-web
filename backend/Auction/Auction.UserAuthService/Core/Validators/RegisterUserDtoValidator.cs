using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using FluentValidation;

namespace Auction.UserAuthService.Core.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.")
                .MaximumLength(User.USER_NAME_MAX_LENGTH).WithMessage($"UserName must be at most {User.USER_NAME_MAX_LENGTH} characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(User.EMAIL_MAX_LENGTH).WithMessage($"Email must be at most {User.EMAIL_MAX_LENGTH} characters.");
            RuleFor(x => x.Contacts)
              .NotEmpty().WithMessage("Contracts is required.")
              .MaximumLength(User.CONTACTS_MAX_LENGTH).WithMessage($"Contracts must be at most {User.CONTACTS_MAX_LENGTH} characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(User.PASSWORD_MiN_LENGTH).WithMessage($"Password must be at least {User.PASSWORD_MiN_LENGTH} characters.")
                .MaximumLength(User.PASSWORD_MAX_LENGTH).WithMessage($"Password must be at most {User.PASSWORD_MAX_LENGTH} characters.");
        }
    }
}
