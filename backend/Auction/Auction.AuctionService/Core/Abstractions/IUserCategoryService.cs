namespace Auction.AuctionService.Core.Abstractions
{
    public interface IUserCategoryService
    {
        Task<string> GetUserCategoryForAuction(Guid userId, Guid auctionId);

    }
}