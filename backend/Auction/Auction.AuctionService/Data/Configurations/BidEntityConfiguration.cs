using Auction.AuctionService.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Auction.AuctionService.Data.Configurations
{
    public class BidEntityConfiguration : IEntityTypeConfiguration<BidEntity>
    {
        public void Configure(EntityTypeBuilder<BidEntity> builder)
        {

            builder.HasKey(b => b.BidId);

            builder.Property(b => b.Value)
                .IsRequired();

            builder.Property(b => b.CreationTime)
                .IsRequired();

            builder.Property(b => b.BidCreatorId)
                .IsRequired();

            builder.Property(b => b.AuctionId)
                .IsRequired();

            builder.HasOne<AuctionDetailsEntity>()
                .WithMany() 
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.AuctionId);
        }
    }

}
