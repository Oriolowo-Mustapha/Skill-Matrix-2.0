using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillGapsAndAssessmentConcept : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Concept",
                table: "Assessments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SkillGaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Concept = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    DateIdentified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillGaps_AssessmentResults_AssessmentResultId",
                        column: x => x.AssessmentResultId,
                        principalTable: "AssessmentResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillGaps_AssignedSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "AssignedSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillGaps_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillGaps_TeamMembers_TeamMemberId",
                        column: x => x.TeamMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillGaps_AssessmentResultId",
                table: "SkillGaps",
                column: "AssessmentResultId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGaps_LearnerId",
                table: "SkillGaps",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGaps_SkillId",
                table: "SkillGaps",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGaps_TeamMemberId",
                table: "SkillGaps",
                column: "TeamMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillGaps");

            migrationBuilder.DropColumn(
                name: "Concept",
                table: "Assessments");
        }
    }
}
