using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using FluentValidation;

namespace Auction.UserAuthService.Core.Validators
{
    public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required and cannot be empty.")
                .EmailAddress().WithMessage("Please enter a valid email address");
           
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required and cannot be empty.")
                .MinimumLength(User.PASSWORD_MiN_LENGTH).WithMessage($"Password must be at least {User.PASSWORD_MiN_LENGTH} characters long.")
                .MaximumLength(User.PASSWORD_MAX_LENGTH).WithMessage($"Password cannot exceed {User.PASSWORD_MAX_LENGTH} characters.");
        }
    }
}
