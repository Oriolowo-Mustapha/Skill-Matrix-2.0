using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalPoints",
                table: "TeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalPoints",
                table: "Learners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PeerEndorsements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EndorserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndorseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    DateEndorsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerEndorsements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerEndorsements_EndorserId_EndorseeId_SkillId",
                table: "PeerEndorsements",
                columns: new[] { "EndorserId", "EndorseeId", "SkillId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerEndorsements");

            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "Learners");
        }
    }
}
