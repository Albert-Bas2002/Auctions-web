
namespace Auction.Core.Abstractions
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var valueObject = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        public override int GetHashCode() =>
            GetEqualityComponents()
                .Aggregate(0, (hashCode, component) =>
                    HashCode.Combine(hashCode, component?.GetHashCode() ?? 0));

        public static bool operator ==(ValueObject? a, ValueObject? b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
    }

}
