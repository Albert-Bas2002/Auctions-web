namespace Auction.UserAuthService.Data.SqlEntities.AuthEntities
{
    public class UserRoleEntity
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public UserEntity UserEntity { get; set; } = null!;
        public RoleEntity RoleEntity { get; set; } = null!;


    }
}
