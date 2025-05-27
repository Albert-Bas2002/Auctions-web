using Auction.UserAuthService.Contracts.Dtos;

namespace Auction.UserAuthService.Contracts
{
    public class UserAuthApiResponse<T>
    {
        public T? Data { get; set; }
        public ErrorDto? Error { get; set; }
    }
}
