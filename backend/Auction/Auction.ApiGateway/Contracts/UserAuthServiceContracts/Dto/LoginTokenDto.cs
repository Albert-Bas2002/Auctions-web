using System.ComponentModel.DataAnnotations;

namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class LoginTokenDto
    {
        public string? Token { get; set; }
    }
}
