using Auction.AuctionService.Core.Abstractions;

namespace Auction.AuctionService.Application.Services
{
    public class UserCategoryService : IUserCategoryService
    {
        private readonly IAuctionDetailsRepository _auctionDetailsRepository;
        private readonly IAuctionStatusRepository _auctionStatusRepository;
        private readonly IBidRepository _bidRepository;


        public UserCategoryService(IAuctionDetailsRepository auctionDetailsRepository,
            IAuctionStatusRepository auctionStatusRepository,
            IBidRepository bidRepository)
        {
            _auctionStatusRepository = auctionStatusRepository;
            _auctionDetailsRepository = auctionDetailsRepository;
            _bidRepository = bidRepository;
        }
        public async Task<string> GetUserCategoryForAuction(Guid userId, Guid auctionId)
        {
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
            var auctionStatus = await _auctionStatusRepository.GetByAuctionId(auctionId);
            var isBidder = await _bidRepository.UserHasBidOnAuction(userId, auctionId);

            if (userId == Guid.Empty)
            {
                return "Guest";

            }
            if (auctionDetails.AuctionCreatorId == userId)
            {
                return "Creator";
            }
            else if (auctionStatus.AuctionWinnerId == userId)
            {
                return "Winner";
            }
            else if (isBidder)
            {
                return "Bidder";
            }
            else { return "Guest"; }
        }
    }
}
