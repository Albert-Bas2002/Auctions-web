using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Auction.AuctionService.Data.Entities
{
    public class AuctionStatusEntity
    {
        public Guid AuctionId { get; set; }
        public bool IsCloseByCreator { get; set; } = false;
        public bool IsCloseByModerator { get; set; } = false;
        public bool HasAuctionWinner { get; set; } = false;
        public Guid AuctionWinnerId { get; set; } = Guid.Empty;
        public bool IsDealCompletedByAuctionWinner { get; set; } = false;
        public bool IsDealCompletedByAuctionCreator { get; set; } = false;
        public bool IsCompletelyFinished { get; set; } = false;
        public AuctionDetailsEntity AuctionDetails { get; set; } = null!;
    }
}
