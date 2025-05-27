using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Models;
using FluentValidation;

namespace Auction.AuctionService.Core.Validators
{


    public class AuctionUpdateDtoValidator : AbstractValidator<AuctionUpdateDto>
    {
        public AuctionUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(AuctionDetails.MAX_TITLE_LENGTH);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(AuctionDetails.MAX_DESCRIPTION_LENGTH);

        }
        }
    } 
