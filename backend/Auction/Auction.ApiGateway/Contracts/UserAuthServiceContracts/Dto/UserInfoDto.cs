using System.ComponentModel.DataAnnotations;

namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class UserInfoDto
    {
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string Contacts { get; set; }


    }
}
