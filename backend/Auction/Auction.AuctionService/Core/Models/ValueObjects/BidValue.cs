using Auction.Core.Abstractions;

namespace Auction.Core.Models.ValueObjects
{
    class BidValue : ValueObject
    {
        public int value { get; }

    private BidValue(int value)
        {
            this.value = value;
        }
    public static BidValue Create(int value)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException("Bid cannot be negative or null.");
        }
        return new BidValue(value);
        }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return value;
    }
    } 
}