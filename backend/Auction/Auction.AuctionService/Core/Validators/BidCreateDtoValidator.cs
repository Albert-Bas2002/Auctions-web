using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Models;
using FluentValidation;

namespace Auction.AuctionService.Core.Validators
{


    public class BidCreateDtoValidator : AbstractValidator<BidCreateDto>
    {
        public BidCreateDtoValidator()
        {
            RuleFor(x => x.BidValue)
                .GreaterThanOrEqualTo(0);


        }
    }
}
