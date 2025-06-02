using Auction.HubService.Core.Models;
using Auction.HubService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Auction.HubService.Data
{
    public class AuctionChatDbContext : DbContext
    {

        public AuctionChatDbContext(DbContextOptions<AuctionChatDbContext> options)
             : base(options)
        {
        }


        public DbSet<AuctionChatMessageEntity> AuctionChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuctionChatMessageEntity>()
               .HasKey(m => m.MessageId);

            modelBuilder.Entity<AuctionChatMessageEntity>()
               .Property(m => m.MessageId)
              .IsRequired();

            modelBuilder.Entity<AuctionChatMessageEntity>()
                .Property(m => m.AuctionId)
                .IsRequired();
            modelBuilder.Entity<AuctionChatMessageEntity>()
                .Property(m => m.SenderId)
                .IsRequired();

            modelBuilder.Entity<AuctionChatMessageEntity>()
                .Property(m => m.Timestamp)
                .IsRequired();

            modelBuilder.Entity<AuctionChatMessageEntity>()
                .Property(m => m.UserCategoryForAuction)
                .HasMaxLength(30);

            modelBuilder.Entity<AuctionChatMessageEntity>()
                .Property(m => m.Message)
                .HasMaxLength(AuctionChatMessage.MAX_MESSAGE_LENGHT);
        }
    }
}
