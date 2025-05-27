using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auction.AuctionService.Application.Services
{
    public class AuctionCreateUpdateService : IAuctionCreateUpdateService
    {
        private readonly IAuctionDetailsRepository _auctionDetailsRepository;
        private readonly IAuctionStatusRepository _auctionStatusRepository;

        public AuctionCreateUpdateService(IAuctionDetailsRepository auctionDetailsRepository, IAuctionStatusRepository auctionStatusRepository)
        {
            _auctionStatusRepository = auctionStatusRepository;
            _auctionDetailsRepository = auctionDetailsRepository;

        }
        public async Task Create(Guid auctionCreatorId, int auctionDurationInDays, string title, string description, int reserve)
        {
            Guid AuctionId = Guid.NewGuid();
            var auctionDetails = AuctionDetails.Create(AuctionId,
                auctionCreatorId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(auctionDurationInDays),
                title,
                description,
                reserve);

            var auctionResult = AuctionStatus.Create(AuctionId);
            await _auctionDetailsRepository.Add(auctionDetails);
            await _auctionStatusRepository.Add(auctionResult);


        }


        public async Task<Result> Update(Guid auctionId,Guid userId, string title, string description)
        {
            if (auctionId == Guid.Empty)
            {
                throw new InvalidOperationException("Auction does not exist.");
            }
            var auctionDetails = await _auctionDetailsRepository.GetByAuctionId(auctionId);
            if (auctionDetails.AuctionCreatorId == userId)
            {
                auctionDetails.ChangeTitle(title);
                auctionDetails.ChangeDescription(description);
                await _auctionDetailsRepository.Update(auctionDetails);
                return Result.Success();
            }
            else return Result.Failure("Auction can only be updated by the creator");


        }
    }
}
