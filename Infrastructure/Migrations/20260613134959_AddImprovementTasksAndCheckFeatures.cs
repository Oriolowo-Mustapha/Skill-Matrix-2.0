using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImprovementTasksAndCheckFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Concept",
                table: "RecommendedResources",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BatchType",
                table: "AssessmentBatches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConceptFocus",
                table: "AssessmentBatches",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImprovementTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImprovementPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Concept = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecommendedResourceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImprovementTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImprovementTasks_ImprovementPlans_ImprovementPlanId",
                        column: x => x.ImprovementPlanId,
                        principalTable: "ImprovementPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImprovementTasks_RecommendedResources_RecommendedResourceId",
                        column: x => x.RecommendedResourceId,
                        principalTable: "RecommendedResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementTasks_ImprovementPlanId",
                table: "ImprovementTasks",
                column: "ImprovementPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementTasks_RecommendedResourceId",
                table: "ImprovementTasks",
                column: "RecommendedResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImprovementTasks");

            migrationBuilder.DropColumn(
                name: "Concept",
                table: "RecommendedResources");

            migrationBuilder.DropColumn(
                name: "BatchType",
                table: "AssessmentBatches");

            migrationBuilder.DropColumn(
                name: "ConceptFocus",
                table: "AssessmentBatches");
        }
    }
}
