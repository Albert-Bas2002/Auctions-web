using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Entities;
using Auction.UserService.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Auction.AuctionService.Data.Repositories
{
    public class AuctionDetailsRepository : IAuctionDetailsRepository
    {
        private readonly AuctionDbContext _context;

        public AuctionDetailsRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task Add(AuctionDetails auctionDetails)
        {

            var existingEntity = await _context.AuctionsDetails
                .FirstOrDefaultAsync(r => r.AuctionId == auctionDetails.AuctionId);

            if (existingEntity != null)
                throw new InvalidOperationException($"Auction already exists.");

            var auctionEntity = new AuctionDetailsEntity
            {
                AuctionId = auctionDetails.AuctionId,
                AuctionCreatorId = auctionDetails.AuctionCreatorId,
                Title = auctionDetails.GetTitle(),
                Description = auctionDetails.GetDescription(),
                CreationTime = auctionDetails.CreationTime,
                EndTime = auctionDetails.EndTime,
                IsActive = auctionDetails.IsActive,
                Reserve = auctionDetails.Reserve
            };

            _context.AuctionsDetails.Add(auctionEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuctionDetails>> GetAllSorted(string sortType, bool isActive, int page)
        {
            var pageSize = 12;
            var auctionsQuery = _context.AuctionsDetails
                .Where(a => a.IsActive == isActive);

            auctionsQuery = sortType?.ToLower() switch
            {
                "creationtime_asc" => auctionsQuery.OrderBy(a => a.CreationTime).ThenBy(a => a.AuctionId),
                "creationtime_desc" => auctionsQuery.OrderByDescending(a => a.CreationTime).ThenBy(a => a.AuctionId),
                "endtime_asc" => auctionsQuery.OrderBy(a => a.EndTime).ThenBy(a => a.AuctionId),
                "endtime_desc" => auctionsQuery.OrderByDescending(a => a.EndTime).ThenBy(a => a.AuctionId),
                "reserve_asc" => auctionsQuery.OrderBy(a => a.Reserve).ThenBy(a => a.AuctionId),
                "reserve_desc" => auctionsQuery.OrderByDescending(a => a.Reserve).ThenBy(a => a.AuctionId),
                _ => auctionsQuery.OrderBy(a => a.CreationTime).ThenBy(a => a.AuctionId)
            };

            var auctionEntities = await auctionsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return auctionEntities
                .Select(a => AuctionDetails.Create(
                    a.AuctionId,
                    a.AuctionCreatorId,
                    a.CreationTime,
                    a.EndTime,
                    a.Title,
                    a.Description,
                    a.Reserve))
                .ToList();
        }

        public async Task<List<AuctionDetails>> GetAll(bool isActive)
        {
            var auctionEntities = await _context.AuctionsDetails
                .Where(a => a.IsActive == isActive)
                .ToListAsync();

            return auctionEntities
                .Select(a => AuctionDetails.Create(
                    a.AuctionId,
                    a.AuctionCreatorId,
                    a.CreationTime,
                    a.EndTime,
                    a.Title,
                    a.Description,
                    a.Reserve))
                .ToList();
        }







        public async Task<AuctionDetails> GetByAuctionId(Guid auctionId)
        {
            return (await GetByAuctionIds(new List<Guid> { auctionId })).FirstOrDefault();

        }
        public async Task<List<AuctionDetails>> GetByAuctionIds(List<Guid> auctionIdList)
        {
            var auctions = await _context.AuctionsDetails
                .Where(a => auctionIdList.Contains(a.AuctionId))
                .ToListAsync();

            var result = auctions.Select(auction =>
            {
                var auctionDetails = AuctionDetails.Create(
                    auction.AuctionId,
                    auction.AuctionCreatorId,
                    auction.CreationTime,
                    auction.EndTime,
                    auction.Title,
                    auction.Description,
                    auction.Reserve
                );
                if (!auction.IsActive) { 
                auctionDetails.SetFalseActive();
                }
                return auctionDetails;
            }).ToList();


            return result;
        }

        public async Task<List<AuctionDetails>> GetAllByCreatorId(Guid creatorId, bool isActive)
        {
            var auctions = await _context.AuctionsDetails
                                         .Where(a => a.AuctionCreatorId == creatorId && a.IsActive == isActive)
                                         .ToListAsync();

            var result = auctions.Select(auction =>
            {
                var auctionDetails = AuctionDetails.Create(
                    auction.AuctionId,
                    auction.AuctionCreatorId,
                    auction.CreationTime,
                    auction.EndTime,
                    auction.Title,
                    auction.Description,
                    auction.Reserve
                );
                if (!auction.IsActive)
                {
                    auctionDetails.SetFalseActive();
                }
                return auctionDetails;
            }).ToList();

            return result;
        }

        public async Task Update(AuctionDetails auctionDetails)
        {
            var existingAuction = await _context.AuctionsDetails
                                         .FirstOrDefaultAsync(a => a.AuctionId == auctionDetails.AuctionId);

            if (existingAuction == null)
            {
                throw new InvalidOperationException($"Auction does not exist.");
            }

            if (existingAuction != null)
            {
                existingAuction.Title = auctionDetails.GetTitle();
                existingAuction.Description = auctionDetails.GetDescription();
                existingAuction.CreationTime = auctionDetails.CreationTime;
                existingAuction.EndTime = auctionDetails.EndTime;
                existingAuction.IsActive = auctionDetails.IsActive;
                existingAuction.Reserve = auctionDetails.Reserve;

                _context.AuctionsDetails.Update(existingAuction);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<AuctionCountDto> GetAuctionCount(bool isActive)
        {
            var count = await _context.AuctionsDetails
                .Where(a => a.IsActive == isActive)
                .CountAsync();

            return new AuctionCountDto { Count = count };
        }




    }
}