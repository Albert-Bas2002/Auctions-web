namespace Auction.ApiGateway.Core.Abstractions
{
    public interface ITokenValidator
    {
        bool IsJwtTokenValid(string token);    }
}