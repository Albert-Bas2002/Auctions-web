using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;

namespace Auction.AuctionService.Application.Services
{
    public class AuctionStatusService : IAuctionStatusService
    {
        private readonly IAuctionDetailsRepository _auctionDetailsRepository;
        private readonly IAuctionStatusRepository _auctionStatusRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IUserCategoryService _userCategoryService;



        public AuctionStatusService(IAuctionDetailsRepository auctionInfoRepository, IUserCategoryService userCategoryService, IAuctionStatusRepository auctionResultRepository, IBidRepository bidRepository )
        {
            _auctionStatusRepository = auctionResultRepository;
            _auctionDetailsRepository = auctionInfoRepository;
            _bidRepository = bidRepository;
            _userCategoryService = userCategoryService;
        }
        public async Task CloseAuction(Guid auctionId, string userType)
        {
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
            var auctionStatus = await _auctionStatusRepository.GetByAuctionId(auctionId);
            if (!auctionDetails.IsActive)
            {
                throw new InvalidOperationException("Auction already close.");
            }
            auctionDetails.Close();
            switch (userType)
            {
                case "Moderator":
                    auctionStatus.CloseByModerator();
                    await _bidRepository.DeleteExceptMaxBidByAuctionId(auctionId);

                    break;

                case "Creator":
                    auctionStatus.CloseByCreator();
                    await _bidRepository.DeleteExceptMaxBidByAuctionId(auctionId);

                    break;

                default:
                    throw new ArgumentException($"Invalid user type: {userType}");
            }

            await _auctionDetailsRepository.Update(auctionDetails);
            await _auctionStatusRepository.Update(auctionStatus);
        }
       
        public async Task<ContactDto> GetWinnerOrCreatorId(Guid auctionId, Guid userId)
        {
            var auctionStatusString = await GetAuctionStatus(auctionId);
            if (
                    auctionStatusString == "Auction Completely Finished" ||
                    auctionStatusString == "Deal Completed by Creator" ||
                    auctionStatusString == "Deal Completed by Winner" ||
                    auctionStatusString == "Auction has a Winner"
)
            {
                var userCategory = await _userCategoryService.GetUserCategoryForAuction(userId,auctionId);
                if (userCategory == "Creator") {
                    var auctionStatus = await _auctionStatusRepository.GetByAuctionId(auctionId);
                    var winnerId = auctionStatus.AuctionWinnerId;
                    return new ContactDto { ContactId = winnerId };
                }
                if (userCategory == "Winner")
                {
                    var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
                    var creatorId = auctionDetails.AuctionCreatorId;
                    return new ContactDto { ContactId = creatorId };

                }
                else { return new ContactDto { ContactId = Guid.Empty }; }

            }
            else { return new ContactDto { ContactId = Guid.Empty }; }
        }
        public async Task AuctionDealComplete(Guid auctionId, Guid userId)
        {
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
            var auctionResult = await _auctionStatusRepository.GetByAuctionId(auctionId);
            if (auctionDetails.IsActive)
            {
                throw new InvalidOperationException("Auction is active.");

            }
            var userCategory =  await _userCategoryService.GetUserCategoryForAuction(userId, auctionId);
            switch (userCategory)
            {
                case "Creator":
                    auctionResult.CompleteDealByCreator();
                   
                    break;

                case "Winner":
                    auctionResult.CompleteDealByWinner();
                   
                    break;

                default:
                    throw new ArgumentException($"Invalid user type: {userCategory}");
            }

            await _auctionStatusRepository.Update(auctionResult);
        }

        public async Task<string> GetAuctionStatus(Guid AuctionId)
        {
            var status = await _auctionStatusRepository.GetByAuctionId(AuctionId);
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(AuctionId);
            if (status.IsCompletelyFinished) return "Auction Completely Finished";
            if (status.IsDealCompletedByAuctionCreator) return "Deal Completed by Creator";
            if (status.IsDealCompletedByAuctionWinner) return "Deal Completed by Winner";
            if (status.HasAuctionWinner) return "Auction has a Winner";
            if (status.IsCloseByModerator) return "Closed by Moderator";
            if (status.IsCloseByCreator) return "Closed by Creator";
            if (auctionDetails.IsActive) return "Active";
            return "Finished without winner";

        }
        public async Task AuctionCheckProcess()
        {
            var expiredAuctions = await _auctionDetailsRepository.GetAll(true);

            foreach (var auction in expiredAuctions)
            {
                if (auction.EndTime <= DateTime.UtcNow)
                {
                    auction.Close();
                    await _auctionDetailsRepository.Update(auction);
                    var winningBid = await _bidRepository.GetMaxByAuctionId(auction.AuctionId);
                    if (winningBid != null && winningBid.GetValue() >= auction.Reserve)
                    {
                        await _bidRepository.DeleteExceptMaxBidByAuctionId(auction.AuctionId);
                        var auctionStatus = await _auctionStatusRepository.GetByAuctionId(auction.AuctionId);
                        if (auctionStatus == null)
                        {

                            continue;
                        }
                        auctionStatus.SetWinner(winningBid.BidCreatorId);
                        await _auctionStatusRepository.Update(auctionStatus);
                    }
                    else
                    {
                        await _bidRepository.DeleteAuctionsBids(auction.AuctionId);
                    }
                }
            }
        }
    }
}