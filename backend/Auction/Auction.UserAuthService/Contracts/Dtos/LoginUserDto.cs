using System.ComponentModel.DataAnnotations;
using Auction.UserAuthService.Core.Models;

namespace Auction.UserAuthService.Contracts.Dtos
{
    public class LoginUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
