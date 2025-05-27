using Auction.AuctionService.Core.Models;
using CSharpFunctionalExtensions;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IBidService
    {
        Task<List<Bid>> GetByAuctionId(Guid AuctionId);
        Task<Bid> GetMaxByAuctionId(Guid AuctionId);
        Task<List<Bid>> GetMaxByAuctionIds(List<Guid> AuctionIdList);
        Task<Bid?> GetUserBidForAuction(Guid auctionId, Guid userId);
        Task<Result> Create(Guid auctionId, Guid userId, int bidValue);
        Task Delete(Guid auctionId, Guid userId);

    }
}