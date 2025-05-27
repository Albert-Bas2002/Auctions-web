using Auction.AuctionService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.AuctionService.Data.Configurations
{
    public class AuctionStatusEntityConfiguration : IEntityTypeConfiguration<AuctionStatusEntity>
    {
        public void Configure(EntityTypeBuilder<AuctionStatusEntity> builder)
        {

            builder.HasKey(r => r.AuctionId);

            builder.Property(r => r.IsCloseByCreator)
                .IsRequired();

            builder.Property(r => r.IsCloseByModerator)
                .IsRequired();

            builder.Property(r => r.HasAuctionWinner)
                .IsRequired();

            builder.Property(r => r.AuctionWinnerId)
                .IsRequired();

            builder.Property(r => r.IsDealCompletedByAuctionWinner)
                .IsRequired();

            builder.Property(r => r.IsDealCompletedByAuctionCreator)
                .IsRequired();

            builder.Property(r => r.IsCompletelyFinished)
                .IsRequired();

            builder.HasOne(r => r.AuctionDetails)
                .WithOne(a => a.AuctionStatus)
                .HasForeignKey<AuctionStatusEntity>(r => r.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
