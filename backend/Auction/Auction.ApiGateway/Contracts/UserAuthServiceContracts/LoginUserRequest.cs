using System.ComponentModel.DataAnnotations;

namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts
{
    public class LoginUserRequest
    {
         public string Email { get; set; }
         public string Password { get; set; }
    }
}
