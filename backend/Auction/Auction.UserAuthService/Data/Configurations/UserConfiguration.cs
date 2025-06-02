using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Data.SqlEntities;
using Auction.UserAuthService.Data.SqlEntities.AuthEntities;

namespace Auction.UserAuthService.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(u => u.UserId);

            builder.Property(u => u.UserName)
           .IsRequired()  
           .HasMaxLength(User.USER_NAME_MAX_LENGTH);  

            builder.Property(u => u.Email)
                .IsRequired()  
                .HasMaxLength(User.EMAIL_MAX_LENGTH);
            builder.Property(u => u.Contacts)
               .IsRequired()
               .HasMaxLength(User.CONTACTS_MAX_LENGTH);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(100);


            builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<UserRoleEntity>(
                ur => ur.HasOne(ur => ur.RoleEntity).WithMany().HasForeignKey(ur => ur.RoleId),
                ur => ur.HasOne(ur => ur.UserEntity).WithMany().HasForeignKey(ur => ur.UserId)
            );
        }
        }


    }

    
