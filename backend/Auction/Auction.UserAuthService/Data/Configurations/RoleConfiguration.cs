using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Auction.UserAuthService.Data.Entities.AuthEntities;

namespace Auction.UserAuthService.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.HasKey(r => r.Id);


            builder.HasMany(r => r.Users)
                    .WithMany(u => u.Roles)
                    .UsingEntity<UserRoleEntity>(
                        ur => ur.HasOne(ur => ur.UserEntity).WithMany().HasForeignKey(ur => ur.UserId),
                        ur => ur.HasOne(ur => ur.RoleEntity).WithMany().HasForeignKey(ur => ur.RoleId)
                    );
            builder.HasMany(r => r.Permissions)   
                    .WithMany(p => p.Roles)
                    .UsingEntity<RolePermissionEntity>(
                        pr => pr.HasOne(ur => ur.PermissionEntity).WithMany().HasForeignKey(pr => pr.PermissionId),
                        pr => pr.HasOne(pr => pr.RoleEntity).WithMany().HasForeignKey(pr => pr.RoleId)

        );
        }
    }

}
