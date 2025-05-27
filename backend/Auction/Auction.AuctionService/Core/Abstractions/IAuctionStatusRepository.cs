using Auction.AuctionService.Core.Models;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IAuctionStatusRepository
    {
        Task Add(AuctionStatus result);
        Task<AuctionStatus> GetByAuctionId(Guid auctionId);
        Task Update(AuctionStatus result);
        Task<List<AuctionStatus>> GetAllByWinnerId(Guid winnerId);
    }

}
