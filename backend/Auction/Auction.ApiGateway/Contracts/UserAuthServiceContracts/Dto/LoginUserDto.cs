using System.ComponentModel.DataAnnotations;

namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class LoginUserDto
    {
         public string Email { get; set; }
        public string Password { get; set; }

    }
}
