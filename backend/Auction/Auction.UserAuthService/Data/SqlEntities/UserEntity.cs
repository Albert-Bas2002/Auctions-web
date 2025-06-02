using Auction.UserAuthService.Data.SqlEntities.AuthEntities;

namespace Auction.UserAuthService.Data.SqlEntities
{
    public class UserEntity
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contacts { get; set; } = string.Empty;


        public ICollection<RoleEntity> Roles { get; set; } = [];

    }

}