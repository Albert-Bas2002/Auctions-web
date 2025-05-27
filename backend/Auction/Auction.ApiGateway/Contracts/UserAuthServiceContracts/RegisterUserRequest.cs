using System.ComponentModel.DataAnnotations;

namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts
{
    public class RegisterUserRequest
    {
         public string UserName { get; set; }
         public string Email { get; set; }
         public string Password { get; set; }
        public string Contacts { get; set; }

    }
}
