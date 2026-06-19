using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerPathTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CareerPathTrackId",
                table: "CareerPathSkill",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CareerPathTrackId",
                table: "AssignedCareerPaths",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CareerPathTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    CareerPathId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerPathTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerPathTracks_CareerPaths_CareerPathId",
                        column: x => x.CareerPathId,
                        principalTable: "CareerPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerPathSkill_CareerPathTrackId",
                table: "CareerPathSkill",
                column: "CareerPathTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCareerPaths_CareerPathTrackId",
                table: "AssignedCareerPaths",
                column: "CareerPathTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerPathTracks_CareerPathId",
                table: "CareerPathTracks",
                column: "CareerPathId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignedCareerPaths_CareerPathTracks_CareerPathTrackId",
                table: "AssignedCareerPaths",
                column: "CareerPathTrackId",
                principalTable: "CareerPathTracks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathSkill_CareerPathTracks_CareerPathTrackId",
                table: "CareerPathSkill",
                column: "CareerPathTrackId",
                principalTable: "CareerPathTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignedCareerPaths_CareerPathTracks_CareerPathTrackId",
                table: "AssignedCareerPaths");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathSkill_CareerPathTracks_CareerPathTrackId",
                table: "CareerPathSkill");

            migrationBuilder.DropTable(
                name: "CareerPathTracks");

            migrationBuilder.DropIndex(
                name: "IX_CareerPathSkill_CareerPathTrackId",
                table: "CareerPathSkill");

            migrationBuilder.DropIndex(
                name: "IX_AssignedCareerPaths_CareerPathTrackId",
                table: "AssignedCareerPaths");

            migrationBuilder.DropColumn(
                name: "CareerPathTrackId",
                table: "CareerPathSkill");

            migrationBuilder.DropColumn(
                name: "CareerPathTrackId",
                table: "AssignedCareerPaths");
        }
    }
}
