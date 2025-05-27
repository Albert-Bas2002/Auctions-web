using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.AuctionService.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuctionsDetails",
                columns: table => new
                {
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionCreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Reserve = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionsDetails", x => x.AuctionId);
                });

            migrationBuilder.CreateTable(
                name: "AuctionsStatus",
                columns: table => new
                {
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCloseByCreator = table.Column<bool>(type: "boolean", nullable: false),
                    IsCloseByModerator = table.Column<bool>(type: "boolean", nullable: false),
                    HasAuctionWinner = table.Column<bool>(type: "boolean", nullable: false),
                    AuctionWinnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDealCompletedByAuctionWinner = table.Column<bool>(type: "boolean", nullable: false),
                    IsDealCompletedByAuctionCreator = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompletelyFinished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionsStatus", x => x.AuctionId);
                    table.ForeignKey(
                        name: "FK_AuctionsStatus_AuctionsDetails_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "AuctionsDetails",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    BidId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidCreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.BidId);
                    table.ForeignKey(
                        name: "FK_Bids_AuctionsDetails_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "AuctionsDetails",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId",
                table: "Bids",
                column: "AuctionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuctionsStatus");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "AuctionsDetails");
        }
    }
}
