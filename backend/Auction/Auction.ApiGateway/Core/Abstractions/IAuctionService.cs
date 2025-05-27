using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts.Dtos;
using CSharpFunctionalExtensions;

namespace Auction.ApiGateway.Core.Abstractions
{
    public interface IAuctionService
    {
        Task<Result> CloseAuction(Guid auctionId, string userType, Guid? userId = null);
        Task<Result> AuctionCompleteDeal(Guid auctionId, Guid userId);
        Task<Result> CreateAuction(Guid userId, AuctionCreateDto auctionCreateDto);
        Task<Result<AuctionPageDto>> GetAuctionPage(Guid auctionId, Guid userId);
        Task<Result<List<AuctionListItemDto>>> GetAuctionsForUser(Guid userId, string category, bool? active = null);
        Task<Result<List<AuctionListItemDto>>> GetAuctions(string? sortType, int page = 1);
        Task<Result> UploadPhotosForAuction(Guid auctionId, List<IFormFile> photos, Guid userId);
        Task<Result> DeletePhotosForAuction(Guid auctionId, int[] indexesToDelete, Guid userId);
        Task<Result> IsAuctionActive(Guid auctionId);
        Task<Result<ContactDto>> GetCreatorOrWinnerId(Guid auctionId, Guid userId);  
    }
}