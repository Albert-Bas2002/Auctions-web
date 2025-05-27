using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using FluentValidation;

namespace Auction.UserAuthService.Core.Validators
{
    public class ChangeContactsDtoValidator : AbstractValidator<ChangeContactsDto>
    {
        public ChangeContactsDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.NewContacts)
                .NotEmpty().WithMessage("Current contacts is required.")
                .MaximumLength(User.CONTACTS_MAX_LENGTH).WithMessage($"Current username must be at most {User.CONTACTS_MAX_LENGTH} characters.");

        }
    }
}
