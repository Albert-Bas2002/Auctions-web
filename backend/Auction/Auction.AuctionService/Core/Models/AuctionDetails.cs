using Auction.Core.Models.ValueObjects;
using FluentValidation;

namespace Auction.AuctionService.Core.Models
{
    public class AuctionDetails
    {
        public const int MAX_TITLE_LENGTH = 100;
        public const int MAX_DESCRIPTION_LENGTH = 1000;
        public const int MAX_AUCTION_DURATION_DAYS = 31;
        public const int MIN_AUCTION_DURATION_DAYS = 3;

        public Guid AuctionId { get; }
        public Guid AuctionCreatorId { get; }
        public DateTime CreationTime { get; }
        public DateTime EndTime { get; private set; }

        private TitleValue Title;
        private DescriptionValue Description;
        public int Reserve { get; } = 0;
        public bool IsActive { get; private set; } = true;


        private AuctionDetails(Guid auctionId, Guid auctionCreatorId, DateTime creationTime, DateTime endTime, string title, string description, int reserve)
        {
            AuctionId = auctionId;
            CreationTime = creationTime;
            AuctionCreatorId = auctionCreatorId;
            EndTime = endTime;
            Title = TitleValue.Create(title);
            Description = DescriptionValue.Create(description);
            Reserve = reserve;
        }
        public static AuctionDetails Create(Guid auctionId, Guid auctionCreatorId, DateTime creationTime, DateTime endTime, string title, string description, int reserve)

        {
            if (auctionId == Guid.Empty)
                throw new ArgumentException("Auction ID cannot be empty.", nameof(auctionId));

            if (auctionCreatorId == Guid.Empty)
                throw new ArgumentException("Auction creator ID cannot be empty.", nameof(auctionCreatorId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            if (title.Length > MAX_TITLE_LENGTH)
                throw new ArgumentException($"Title cannot be longer than {MAX_TITLE_LENGTH} characters.", nameof(title));

            if (description.Length > MAX_DESCRIPTION_LENGTH)
                throw new ArgumentException($"Description cannot be longer than {MAX_DESCRIPTION_LENGTH} characters.", nameof(description));

            if (endTime <= creationTime)
                throw new ArgumentException("End time must be after creation time.", nameof(endTime));

            if (reserve < 0)
                throw new ArgumentException("Reserve price cannot be negative.", nameof(reserve));
            return new AuctionDetails(auctionId, auctionCreatorId, creationTime, endTime, title, description, reserve);
        }


        public void ChangeTitle(string newTitle)
        {
            if (!IsActive)
                throw new InvalidOperationException("Auction is already closed.");

            if (newTitle.Length > MAX_TITLE_LENGTH)
                throw new ArgumentException($"Title cannot be longer than {MAX_TITLE_LENGTH} characters.");
            Title = TitleValue.Create(newTitle);
        }

        public void ChangeDescription(string newDescription)
        {
            if (!IsActive)
                throw new InvalidOperationException("Auction is already closed.");

            if (newDescription.Length > MAX_DESCRIPTION_LENGTH)
                throw new ArgumentException($"Description cannot be longer than {MAX_DESCRIPTION_LENGTH} characters.");

            Description = DescriptionValue.Create( newDescription);
        }

        public void Close()
        {
            if (!IsActive)
                throw new InvalidOperationException("Auction is already closed.");

          IsActive=false;
          EndTime = DateTime.UtcNow;
        }
        public void SetFalseActive()
        {
            if (!IsActive)
                throw new InvalidOperationException("Auction is already closed.");

            IsActive = false;
        }
        public string GetTitle()
        {
            return Title.Title;
        }
        public string GetDescription()
        {
            return Description.Description;
        }

    }
}
