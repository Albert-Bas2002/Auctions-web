using Auction.Core.Abstractions;
using Auction.AuctionService.Core.Models;
namespace Auction.Core.Models.ValueObjects

{
    public class TitleValue : ValueObject
    {
        public string Title { get; }
        private TitleValue(string title)
        {
            Title=title;       
        }
        public static TitleValue Create(string title)
        {
            if (title.Length > AuctionDetails.MAX_TITLE_LENGTH)
                throw new ArgumentException($"Title cannot be longer than {AuctionDetails.MAX_TITLE_LENGTH} characters.");
            return new TitleValue(title);
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Title;
        }

    }
}
