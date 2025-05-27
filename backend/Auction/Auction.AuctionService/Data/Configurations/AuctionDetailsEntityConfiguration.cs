using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.AuctionService.Data.Configurations
{
    public class AuctionDetailsEntityConfiguration : IEntityTypeConfiguration<AuctionDetailsEntity>
    {
        public void Configure(EntityTypeBuilder<AuctionDetailsEntity> builder)
        {

            builder.HasKey(a => a.AuctionId);

            builder.Property(a => a.AuctionId)
                .IsRequired();

            builder.Property(a => a.AuctionCreatorId)
                .IsRequired();

            builder.Property(a => a.Title)
                .HasMaxLength(AuctionDetails.MAX_TITLE_LENGTH)
                .IsRequired();

            builder.Property(a => a.Description)
                .HasMaxLength(AuctionDetails.MAX_DESCRIPTION_LENGTH);

            builder.Property(a => a.CreationTime)
                .IsRequired();

            builder.Property(a => a.EndTime)
                .IsRequired();

            builder.Property(a => a.IsActive)
                .IsRequired();

            builder.Property(a => a.Reserve)
                   .IsRequired()
                   .HasDefaultValue(0);
        }
    }
}
