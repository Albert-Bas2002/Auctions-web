using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;

namespace Auction.AuctionService.Application.Services
{
    public class AuctionGetDetailsService : IAuctionGetDetailsService
    {
        private readonly IAuctionDetailsRepository _auctionDetailsRepository;
        private readonly IAuctionStatusRepository _auctionStatusRepository;
        private readonly IBidRepository _bidRepository;


        public AuctionGetDetailsService(IAuctionDetailsRepository auctionDetailsRepository,
            IAuctionStatusRepository auctionStatusRepository,
            IBidRepository bidRepository)
        {
            _auctionStatusRepository = auctionStatusRepository;
            _auctionDetailsRepository = auctionDetailsRepository;
            _bidRepository = bidRepository;


        }
        public async Task<List<AuctionDetails>> GetAll(string sortType, bool isActive, int page)
        {
            return await _auctionDetailsRepository.GetAllSorted(sortType, isActive, page);
        }


        public async Task<AuctionDetails> GetById(Guid auctionId)
        {
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
            return auctionDetails;
        }
        public async Task<List<AuctionDetails>> GetByIds(List<Guid> auctionId)
        {
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionIds(auctionId);
            return auctionDetails;
        }
        public async Task<List<AuctionDetails>> GetByCreatorId(Guid creatorId, bool isActive)
        {
            var auctionsDetails = await _auctionDetailsRepository.GetAllByCreatorId(creatorId, isActive);
            return auctionsDetails;
        }
        public async Task<List<AuctionDetails>> GetByWinnerId(Guid winnerId)
        {
            var auctionsStatus = await _auctionStatusRepository.GetAllByWinnerId(winnerId);
            var wonAuctionIds = auctionsStatus.Select(a => a.AuctionId).ToList();
            var auctionsDetails =await GetByIds(wonAuctionIds);
            return auctionsDetails;
        }

        public async Task<List<AuctionDetails>> GetByBiddersId(Guid userId)
        {
            var usersBids = await _bidRepository.GetAllByUserId(userId);
            var auctionIds = usersBids
                .Select(ad => ad.AuctionId)
                .ToList();
            var auctionsDetails = await GetByIds(auctionIds);
            var auctionsDetailsActive = auctionsDetails
                .Where(ad => ad.IsActive == true)
                .ToList();
            return auctionsDetailsActive;
        }
        public async Task<AuctionCountDto> GetAuctionCount(bool isActive)
        {
            var count = await _auctionDetailsRepository.GetAuctionCount(isActive);
            return count;
        }

    }
}
