using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimerAndCodingToAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedOutput",
                table: "Assessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                table: "Assessments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "AssessmentBatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "AssessmentBatches",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedOutput",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "AssessmentBatches");

            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "AssessmentBatches");
        }
    }
}
