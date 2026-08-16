using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGranularAssessmentStateAndAuthoritativeTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserResponses_AssessmentBatchId",
                table: "UserResponses");

            migrationBuilder.AddColumn<bool>(
                name: "IsFlagged",
                table: "UserResponses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserResponses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "AssessmentBatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastActiveQuestionIndex",
                table: "AssessmentBatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_AssessmentBatchId_AssessmentQuestionId",
                table: "UserResponses",
                columns: new[] { "AssessmentBatchId", "AssessmentQuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserResponses_AssessmentBatchId_AssessmentQuestionId",
                table: "UserResponses");

            migrationBuilder.DropColumn(
                name: "IsFlagged",
                table: "UserResponses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserResponses");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "AssessmentBatches");

            migrationBuilder.DropColumn(
                name: "LastActiveQuestionIndex",
                table: "AssessmentBatches");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_AssessmentBatchId",
                table: "UserResponses",
                column: "AssessmentBatchId");
        }
    }
}
