using Auction.AuctionService.Core.Models;
using Auction.Core.Abstractions;
namespace Auction.Core.Models.ValueObjects

{
    public class DescriptionValue : ValueObject
    {
        public string Description { get; }
        private DescriptionValue(string description)
        {
            Description = description;
        }
        public static DescriptionValue Create(string description)
        {
            if (description.Length > AuctionDetails.MAX_DESCRIPTION_LENGTH)
            {
                throw new ArgumentException($"Description cannot be longer than {AuctionDetails.MAX_DESCRIPTION_LENGTH} characters.");
            }
            return new DescriptionValue(description);
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Description;
        }

    }
}
