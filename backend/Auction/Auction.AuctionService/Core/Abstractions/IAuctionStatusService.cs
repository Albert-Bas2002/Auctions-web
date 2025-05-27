using Auction.AuctionService.Contracts.Dtos;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IAuctionStatusService
    {
        Task AuctionDealComplete(Guid auctionId, Guid userId);
        Task CloseAuction(Guid auctionId, string typeUser);
        Task<string> GetAuctionStatus(Guid AuctionId);
         Task AuctionCheckProcess();
        Task<ContactDto> GetWinnerOrCreatorId(Guid auctionId, Guid userId);
    }
}