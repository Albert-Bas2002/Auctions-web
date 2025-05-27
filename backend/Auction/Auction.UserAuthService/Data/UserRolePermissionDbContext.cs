using Microsoft.EntityFrameworkCore;
using Auction.UserAuthService.Data.Configurations;
using Microsoft.Extensions.Configuration;
using Auction.UserAuthService.Data.Entities;
using Auction.UserAuthService.Data.Entities.AuthEntities;

namespace Auction.UserAuthService.Data
{
    public class UserRolePermissionDbContext : DbContext
    {


        private readonly IConfiguration _configuration;
        public UserRolePermissionDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString(nameof(UserRolePermissionDbContext)));

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

