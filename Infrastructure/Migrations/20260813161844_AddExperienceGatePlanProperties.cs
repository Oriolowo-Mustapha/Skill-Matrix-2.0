using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceGatePlanProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImprovementPlans_AssessmentResults_AssessmentResultId",
                table: "ImprovementPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssessmentResultId",
                table: "ImprovementPlans",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedSkillId",
                table: "ImprovementPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarterPlan",
                table: "ImprovementPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementPlans_AssignedSkillId",
                table: "ImprovementPlans",
                column: "AssignedSkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImprovementPlans_AssessmentResults_AssessmentResultId",
                table: "ImprovementPlans",
                column: "AssessmentResultId",
                principalTable: "AssessmentResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImprovementPlans_AssignedSkills_AssignedSkillId",
                table: "ImprovementPlans",
                column: "AssignedSkillId",
                principalTable: "AssignedSkills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImprovementPlans_AssessmentResults_AssessmentResultId",
                table: "ImprovementPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_ImprovementPlans_AssignedSkills_AssignedSkillId",
                table: "ImprovementPlans");

            migrationBuilder.DropIndex(
                name: "IX_ImprovementPlans_AssignedSkillId",
                table: "ImprovementPlans");

            migrationBuilder.DropColumn(
                name: "AssignedSkillId",
                table: "ImprovementPlans");

            migrationBuilder.DropColumn(
                name: "IsStarterPlan",
                table: "ImprovementPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssessmentResultId",
                table: "ImprovementPlans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ImprovementPlans_AssessmentResults_AssessmentResultId",
                table: "ImprovementPlans",
                column: "AssessmentResultId",
                principalTable: "AssessmentResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
