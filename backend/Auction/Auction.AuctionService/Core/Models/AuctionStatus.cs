namespace Auction.AuctionService.Core.Models
{
    public class AuctionStatus
    {
        public Guid AuctionId { get; }

        public bool IsCloseByCreator { get; private set; } = false;
        public bool IsCloseByModerator { get; private set; } = false;
        public bool HasAuctionWinner { get; private set; } = false;
        public Guid AuctionWinnerId { get; private set; } = Guid.Empty;
        public bool IsDealCompletedByAuctionWinner { get; private set; } = false;
        public bool IsDealCompletedByAuctionCreator { get; private set; } = false;
        public bool IsCompletelyFinished { get; private set; } = false;

        private AuctionStatus(Guid auctionId)
        {
            AuctionId = auctionId;
        }

        public static AuctionStatus Create(Guid auctionId)
        {
            if (auctionId == Guid.Empty)
                throw new ArgumentException("Auction ID cannot be empty.", nameof(auctionId));

            return new AuctionStatus(auctionId);
        }

        public void CloseByCreator()
        {
            if (IsCloseByCreator || IsCloseByModerator)
                throw new InvalidOperationException("The auction is already closed.");

            IsCloseByCreator = true;
        }

        public void CloseByModerator()
        {
            if (IsCloseByCreator || IsCloseByModerator)
                throw new InvalidOperationException("The auction is already closed.");

            IsCloseByModerator = true;
        }

        public void SetWinner(Guid winnerId)
        {
            if (IsCloseByCreator || IsCloseByModerator)
                throw new InvalidOperationException("Cannot set a winner for a closed auction.");

            HasAuctionWinner = true;
            AuctionWinnerId = winnerId;
        }

        public void CompleteDealByWinner()
        {
            if (!HasAuctionWinner)
                throw new InvalidOperationException("There is no winner to complete the deal.");

            if (IsCloseByCreator || IsCloseByModerator)
                throw new InvalidOperationException("Cannot complete the deal on a closed auction.");

            IsDealCompletedByAuctionWinner = true;
            if (IsDealCompletedByAuctionCreator)
            {
                IsCompletelyFinished = true;
            }
        }

        public void CompleteDealByCreator()
        {
            if (!HasAuctionWinner)
                throw new InvalidOperationException("There is no winner to complete the deal.");

            if (IsCloseByCreator || IsCloseByModerator)
                throw new InvalidOperationException("Cannot complete the deal on a closed auction.");

            IsDealCompletedByAuctionCreator = true;
            if (IsDealCompletedByAuctionWinner)
            {
                IsCompletelyFinished = true;
            }
        }
    }
}
