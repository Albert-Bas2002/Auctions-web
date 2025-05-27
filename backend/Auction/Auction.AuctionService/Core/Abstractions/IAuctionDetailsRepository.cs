using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Models;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IAuctionDetailsRepository
    {
        Task Add(AuctionDetails auctionDetails);
        Task<List<AuctionDetails>> GetAllByCreatorId(Guid creatorId, bool isActive);
        Task<List<AuctionDetails>> GetAllSorted(string sortType, bool isActive, int page);
        Task<AuctionDetails> GetByAuctionId(Guid auctionId);
        Task<List<AuctionDetails>> GetByAuctionIds(List<Guid> auctionIdList);
        Task Update(AuctionDetails auctionDetails);
        Task<AuctionCountDto> GetAuctionCount(bool isActive);
        Task<List<AuctionDetails>> GetAll(bool isActive);
    }
}