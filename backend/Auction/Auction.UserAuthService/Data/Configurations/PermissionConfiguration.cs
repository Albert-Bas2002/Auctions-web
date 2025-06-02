using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Auction.UserAuthService.Data.SqlEntities.AuthEntities;

namespace Auction.UserAuthService.Data.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
    {
        public void Configure(EntityTypeBuilder<PermissionEntity> builder)
        {
            builder.HasKey(p => p.Id);


            builder.HasMany(p => p.Roles)
                   .WithMany(r => r.Permissions)
                   .UsingEntity<RolePermissionEntity>(
                       pr => pr.HasOne(pr => pr.RoleEntity).WithMany().HasForeignKey(pr => pr.RoleId),
                       pr => pr.HasOne(ur => ur.PermissionEntity).WithMany().HasForeignKey(pr => pr.PermissionId)

       );
        }
    }
}

