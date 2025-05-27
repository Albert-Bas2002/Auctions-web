using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using FluentValidation;

namespace Auction.UserAuthService.Core.Validators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.PreviousPassword)
                .NotEmpty().WithMessage("Previous password is required.")
                .MinimumLength(User.PASSWORD_MiN_LENGTH).WithMessage($"Previous password must be at least {User.PASSWORD_MiN_LENGTH} characters.")
                .MaximumLength(User.PASSWORD_MAX_LENGTH).WithMessage($"Previous password must be at most {User.PASSWORD_MAX_LENGTH} characters.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(User.PASSWORD_MiN_LENGTH).WithMessage($"New password must be at least {User.PASSWORD_MiN_LENGTH} characters.")
                .MaximumLength(User.PASSWORD_MAX_LENGTH).WithMessage($"New password must be at most {User.PASSWORD_MAX_LENGTH} characters.")
                .NotEqual(x => x.PreviousPassword).WithMessage("New password cannot be the same as the previous password.");
        }
    }
}
