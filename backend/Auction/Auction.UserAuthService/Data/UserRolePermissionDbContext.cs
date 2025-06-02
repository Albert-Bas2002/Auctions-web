using Microsoft.EntityFrameworkCore;
using Auction.UserAuthService.Data.Configurations;
using Auction.UserAuthService.Data.SqlEntities;
using Auction.UserAuthService.Data.SqlEntities.AuthEntities;

namespace Auction.UserAuthService.Data
{
    public class UserRolePermissionDbContext : DbContext
    {
        public UserRolePermissionDbContext(DbContextOptions<UserRolePermissionDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<PermissionEntity> Permissions { get; set; }
        public DbSet<RolePermissionEntity> RolePermission { get; set; }
        public DbSet<UserRoleEntity> UserRole { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());

            modelBuilder.Seed();

            base.OnModelCreating(modelBuilder);
        }
    }
}
