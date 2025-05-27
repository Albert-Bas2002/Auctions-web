using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using CSharpFunctionalExtensions;

namespace Auction.AuctionService.Application.Services
{
    public class BidService : IBidService
    {

        private readonly IBidRepository _bidRepository;
        private readonly IAuctionDetailsRepository _auctionDetailsRepository;

        public BidService(IAuctionDetailsRepository auctionDetailsRepository,
            IAuctionStatusRepository auctionStatusRepository,
            IBidRepository bidRepository)
        {
            _bidRepository = bidRepository;
            _auctionDetailsRepository = auctionDetailsRepository;
        }
        public async Task<List<Bid>> GetByAuctionId(Guid AuctionId)
        {
            var bids = await _bidRepository.GetAllByAuctionId(AuctionId);
            return bids;
        }
        public async Task<Bid?> GetMaxByAuctionId(Guid AuctionId)
        {
            var bid = await _bidRepository.GetMaxByAuctionId(AuctionId);
            return bid;
        }
        public async Task<List<Bid>> GetMaxByAuctionIds(List<Guid> AuctionIdList)
        {
            var bids = await _bidRepository.GetMaxByAuctionIds(AuctionIdList);
            return bids;
        }
        public async Task<Bid?> GetUserBidForAuction(Guid auctionId, Guid userId)
        {
            

            var usersBids = await _bidRepository.GetAllByUserId(userId);

            var userBid = usersBids
                .Where(b => b.AuctionId == auctionId)
                .FirstOrDefault();
            return userBid;
        }
        public async Task<Result> Create(Guid auctionId, Guid userId, int bidValue)
        {
            var auctionMaxBid = await _bidRepository.GetMaxByAuctionId(auctionId);
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
            if (!auctionDetails.IsActive)
            {
                throw new InvalidOperationException("Auction is not Active");
            }
            var isBidderCreatodOfAuction = auctionDetails.AuctionCreatorId == userId;
            if (isBidderCreatodOfAuction)
            {
                throw new InvalidOperationException("The auction creator is not allowed to place a bid on their own auction.");
            }
            if (bidValue <= 0)
            {
                return Result.Failure("Bid value must be greater than zero.");
            }

            if (auctionMaxBid == null || bidValue > auctionMaxBid.GetValue())
            {
                var newBid = Bid.Create(Guid.NewGuid(), userId, auctionId, bidValue, DateTime.UtcNow);
                await _bidRepository.Add(newBid);
                await _bidRepository.DeleteForUserExceptMaxBid(auctionId, userId);
                return Result.Success();
            }
            else
            {
                return Result.Failure("New bid must be greater than the previous bid.");
            }

        }

        public async Task Delete(Guid auctionId, Guid userId)
        {
            await _bidRepository.DeleteBiddersBids(auctionId, userId);
        }
    }
}
