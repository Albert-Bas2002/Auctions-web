namespace Auction.UserAuthService.Data.SqlEntities.AuthEntities
{
    public class RoleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<UserEntity> Users { get; set; } = [];
        public ICollection<PermissionEntity> Permissions { get; set; } = [];
    }
}
