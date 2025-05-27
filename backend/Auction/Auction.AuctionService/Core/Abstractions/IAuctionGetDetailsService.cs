using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Models;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IAuctionGetDetailsService
    {
        Task<List<AuctionDetails>> GetAll(string sortType, bool isActive, int page);
        Task<List<AuctionDetails>> GetByBiddersId(Guid userId);
        Task<List<AuctionDetails>> GetByCreatorId(Guid creatorId, bool isActive);
        Task<AuctionDetails> GetById(Guid auctionId);
        Task<List<AuctionDetails>> GetByIds(List<Guid> auctionId);
        Task<List<AuctionDetails>> GetByWinnerId(Guid winnerId);
        Task<AuctionCountDto> GetAuctionCount(bool isActive);
    }
}