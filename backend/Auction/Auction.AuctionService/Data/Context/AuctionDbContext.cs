using Microsoft.EntityFrameworkCore;
using Auction.AuctionService.Data.Entities;
using Microsoft.Extensions.Configuration;
using Auction.AuctionService.Data.Configurations;

namespace Auction.UserService.Data.Contexts
{
    public class AuctionDbContext : DbContext
    {


        public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
           : base(options)
        {
        }

        public DbSet<AuctionDetailsEntity> AuctionsDetails { get; set; }
        public DbSet<AuctionStatusEntity> AuctionsStatus { get; set; }
        public DbSet<BidEntity> Bids { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AuctionDetailsEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AuctionStatusEntityConfiguration());
            modelBuilder.ApplyConfiguration(new BidEntityConfiguration());


            base.OnModelCreating(modelBuilder);
        }

    }

}

