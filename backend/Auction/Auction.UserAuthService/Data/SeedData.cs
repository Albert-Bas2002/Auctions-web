using Microsoft.EntityFrameworkCore;
using Auction.UserAuthService.Data.SqlEntities.AuthEntities;

namespace Auction.UserAuthService.Data
    {
        public static class SeedData
        {
            public static void Seed(this ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PermissionEntity>().HasData(
                    new PermissionEntity { Id = 1, Name = "Moderator-Permission" },
                    new PermissionEntity { Id = 2, Name = "User-Permission" }

                );

                modelBuilder.Entity<RoleEntity>().HasData(
                    new RoleEntity { Id = 1, Name = "Moderator" },
                    new RoleEntity { Id = 2, Name = "CommonUser" }
                );

                modelBuilder.Entity<RolePermissionEntity>().HasData(
                    new RolePermissionEntity { RoleId = 1, PermissionId = 1 },
                    new RolePermissionEntity { RoleId = 2, PermissionId = 2 }
                    
                );
            }
        }
    }


