using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightedScoresAndVerificationStatusToAssessmentResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CodingScore",
                table: "AssessmentResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "McqScore",
                table: "AssessmentResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlacedProficiencyLevel",
                table: "AssessmentResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "AssessmentResults",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodingScore",
                table: "AssessmentResults");

            migrationBuilder.DropColumn(
                name: "McqScore",
                table: "AssessmentResults");

            migrationBuilder.DropColumn(
                name: "PlacedProficiencyLevel",
                table: "AssessmentResults");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "AssessmentResults");
        }
    }
}
