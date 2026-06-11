using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IconURL = table.Column<string>(type: "text", nullable: false),
                    Criteria = table.Column<string>(type: "text", nullable: false),
                    ProficiencyLevel = table.Column<string>(type: "text", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IconURL = table.Column<string>(type: "text", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Learners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Managers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CareerPathSkill",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CareerPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerPathSkill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerPathSkill_CareerPaths_CareerPathId",
                        column: x => x.CareerPathId,
                        principalTable: "CareerPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CareerPathSkill_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "integer", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignedCareerPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    TeamMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CareerPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateAssigned = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedCareerPaths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignedCareerPaths_CareerPaths_CareerPathId",
                        column: x => x.CareerPathId,
                        principalTable: "CareerPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedCareerPaths_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedCareerPaths_TeamMembers_TeamMemberId",
                        column: x => x.TeamMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignedSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "integer", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateAssigned = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignedSkills_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedSkills_TeamMembers_TeamMemberId",
                        column: x => x.TeamMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LearnerID = table.Column<Guid>(type: "uuid", nullable: true),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamMemberID = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentStatus = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentBatches_AssignedSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "AssignedSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentBatches_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentBatches_TeamMembers_TeamMemberID",
                        column: x => x.TeamMemberID,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignedBadges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    LearnerID = table.Column<Guid>(type: "uuid", nullable: true),
                    DateAwarded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedSkillId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignedBadges_AssignedSkills_AssignedSkillId",
                        column: x => x.AssignedSkillId,
                        principalTable: "AssignedSkills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssignedBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedBadges_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedBadges_TeamMembers_TeamMemberId",
                        column: x => x.TeamMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerID = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamMemberID = table.Column<Guid>(type: "uuid", nullable: true),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "integer", nullable: false),
                    AssessmentBatchId = table.Column<int>(type: "integer", nullable: false),
                    ImprovementPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    NoOfCorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    NoOfWrongAnswers = table.Column<int>(type: "integer", nullable: false),
                    NoOfUnansweredQuestions = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedSkillId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_AssessmentBatches_AssessmentBatchId",
                        column: x => x.AssessmentBatchId,
                        principalTable: "AssessmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_AssignedSkills_AssignedSkillId",
                        column: x => x.AssignedSkillId,
                        principalTable: "AssignedSkills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssessmentResults_AssignedSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "AssignedSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_TeamMembers_TeamMemberID",
                        column: x => x.TeamMemberID,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Questions = table.Column<string>(type: "text", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: false),
                    AssessmentBatchId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_AssessmentBatches_AssessmentBatchId",
                        column: x => x.AssessmentBatchId,
                        principalTable: "AssessmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImprovementPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedSummary = table.Column<string>(type: "text", nullable: false),
                    FocusArea = table.Column<string>(type: "text", nullable: false),
                    DateGenerated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessmentResultId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImprovementPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImprovementPlans_AssessmentResults_AssessmentResultId",
                        column: x => x.AssessmentResultId,
                        principalTable: "AssessmentResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    AssessmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentOptions_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendedResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ResourseType = table.Column<int>(type: "integer", nullable: false),
                    ImprovementPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendedResources_ImprovementPlans_ImprovementPlanId",
                        column: x => x.ImprovementPlanId,
                        principalTable: "ImprovementPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentBatchId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentQuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserResponses_AssessmentBatches_AssessmentBatchId",
                        column: x => x.AssessmentBatchId,
                        principalTable: "AssessmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserResponses_AssessmentOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "AssessmentOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserResponses_Assessments_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserResponses_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserResponses_TeamMembers_TeamMemberId",
                        column: x => x.TeamMemberId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "DateJoined", "Email", "FirstName", "LastName", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiry", "Role", "UserName" },
                values: new object[] { new Guid("f4b1f8b4-5b4a-4b4a-8b4a-5b4a4b4a4b4a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "skillmatrix77@gmail.com", "Super", "Admin", "$2a$12$U6EhdNjpXotPZ04t54w.ZeNsMONXMidmU1WMPjOdAehb5OylKpZK2", null, null, "SuperAdmin", "Superadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentBatches_LearnerID",
                table: "AssessmentBatches",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentBatches_SkillId",
                table: "AssessmentBatches",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentBatches_TeamMemberID",
                table: "AssessmentBatches",
                column: "TeamMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_AssessmentId",
                table: "AssessmentOptions",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_AssessmentBatchId",
                table: "AssessmentResults",
                column: "AssessmentBatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_AssignedSkillId",
                table: "AssessmentResults",
                column: "AssignedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_LearnerID",
                table: "AssessmentResults",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_SkillId",
                table: "AssessmentResults",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_TeamMemberID",
                table: "AssessmentResults",
                column: "TeamMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_AssessmentBatchId",
                table: "Assessments",
                column: "AssessmentBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedBadges_AssignedSkillId",
                table: "AssignedBadges",
                column: "AssignedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedBadges_BadgeId",
                table: "AssignedBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedBadges_LearnerID",
                table: "AssignedBadges",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedBadges_TeamMemberId",
                table: "AssignedBadges",
                column: "TeamMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCareerPaths_CareerPathId",
                table: "AssignedCareerPaths",
                column: "CareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCareerPaths_LearnerId",
                table: "AssignedCareerPaths",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCareerPaths_TeamMemberId",
                table: "AssignedCareerPaths",
                column: "TeamMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedSkills_LearnerId",
                table: "AssignedSkills",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedSkills_SkillId",
                table: "AssignedSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedSkills_TeamMemberId",
                table: "AssignedSkills",
                column: "TeamMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerPathSkill_CareerPathId",
                table: "CareerPathSkill",
                column: "CareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerPathSkill_SkillId",
                table: "CareerPathSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementPlans_AssessmentResultId",
                table: "ImprovementPlans",
                column: "AssessmentResultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Learners_Email",
                table: "Learners",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_Email",
                table: "Managers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_OrganizationId",
                table: "Managers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedResources_ImprovementPlanId",
                table: "RecommendedResources",
                column: "ImprovementPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Email",
                table: "TeamMembers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_ManagerId",
                table: "TeamMembers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_OrganizationId",
                table: "TeamMembers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_AssessmentBatchId",
                table: "UserResponses",
                column: "AssessmentBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_AssessmentQuestionId",
                table: "UserResponses",
                column: "AssessmentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_LearnerId",
                table: "UserResponses",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_SelectedOptionId",
                table: "UserResponses",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_TeamMemberId",
                table: "UserResponses",
                column: "TeamMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "AssignedBadges");

            migrationBuilder.DropTable(
                name: "AssignedCareerPaths");

            migrationBuilder.DropTable(
                name: "CareerPathSkill");

            migrationBuilder.DropTable(
                name: "RecommendedResources");

            migrationBuilder.DropTable(
                name: "UserResponses");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "CareerPaths");

            migrationBuilder.DropTable(
                name: "ImprovementPlans");

            migrationBuilder.DropTable(
                name: "AssessmentOptions");

            migrationBuilder.DropTable(
                name: "AssessmentResults");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "AssessmentBatches");

            migrationBuilder.DropTable(
                name: "AssignedSkills");

            migrationBuilder.DropTable(
                name: "Learners");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
