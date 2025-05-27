using CSharpFunctionalExtensions;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IAuctionCreateUpdateService
    {
        Task Create(Guid auctionCreatorId, int auctionDurationInDays, string title, string description, int reserve);
        Task<Result> Update(Guid auctionId, Guid userId, string title, string description);

    }
}