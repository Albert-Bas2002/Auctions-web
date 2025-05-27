using System.ComponentModel.DataAnnotations;

namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class RegisterUserDto
    {
         public string UserName { get; set; }
         public string Email { get; set; }
         public string Password { get; set; }
        public string Contacts { get; set; }

    }
}

