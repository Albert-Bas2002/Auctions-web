using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using FluentValidation;

namespace Auction.UserAuthService.Core.Validators
{
    public class ChangeUserNameDtoValidator : AbstractValidator<ChangeUserNameDto>
    {
        public ChangeUserNameDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.NewUserName)
                .NotEmpty().WithMessage("Current username is required.")
                .MaximumLength(User.USER_NAME_MAX_LENGTH).WithMessage($"Current username must be at most {User.USER_NAME_MAX_LENGTH} characters.");

        }
    }
}
