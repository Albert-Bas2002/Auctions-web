using System.ComponentModel.DataAnnotations;
using Auction.UserAuthService.Core.Models;

namespace Auction.UserAuthService.Contracts.Dtos
{
    public class RegisterUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Contacts { get; set; }

    }
}
