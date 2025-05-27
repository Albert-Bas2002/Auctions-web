using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Models;
using FluentValidation;

public class AuctionCreateDtoValidator : AbstractValidator<AuctionCreateDto>
{
    public AuctionCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(AuctionDetails.MAX_TITLE_LENGTH);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(AuctionDetails.MAX_DESCRIPTION_LENGTH);

        RuleFor(x => x.Reserve)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AuctionDurationInDays)
            .NotEmpty()
            .InclusiveBetween(AuctionDetails.MIN_AUCTION_DURATION_DAYS, AuctionDetails.MAX_AUCTION_DURATION_DAYS);
    }
}
