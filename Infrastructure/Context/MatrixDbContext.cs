using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
	public class MatrixDbContext : DbContext
	{
		public MatrixDbContext(DbContextOptions<MatrixDbContext> options) : base(options) { }

		public DbSet<Admin> Admins { get; set; }
		public DbSet<Assessment> Assessments { get; set; }
		public DbSet<AssessmentOptions> AssessmentOptions { get; set; }
		public DbSet<AssessmentBatch> AssessmentBatches { get; set; }
		public DbSet<AssessmentResult> AssessmentResults { get; set; }
		public DbSet<Badge> Badges { get; set; }
		public DbSet<CareerPath> CareerPaths { get; set; }
		public DbSet<Learner> Learners { get; set; }
		public DbSet<Manager> Managers { get; set; }
		public DbSet<Organization> Organizations { get; set; }
		public DbSet<Skill> Skills { get; set; }
		public DbSet<TeamMember> TeamMembers { get; set; }
		public DbSet<UserResponse> UserResponses { get; set; }
		public DbSet<AssignedSkill> AssignedSkills { get; set; }
		public DbSet<AssignedBadge> AssignedBadges { get; set; }
		public DbSet<AssignedCareerPath> AssignedCareerPaths { get; set; }
		public DbSet<ImprovementPlan> ImprovementPlans { get; set; }
		public DbSet<RecommendedResource> RecommendedResources { get; set; }
		public DbSet<PeerEndorsement> PeerEndorsements { get; set; }
		public DbSet<SkillGap> SkillGaps { get; set; }
		public DbSet<ImprovementTask> ImprovementTasks { get; set; }
		public DbSet<CareerPathTrack> CareerPathTracks { get; set; }
		public DbSet<UserActivityLog> UserActivityLogs { get; set; }
		public DbSet<UserStreak> UserStreaks { get; set; }
		public DbSet<XpAction> XpActions { get; set; }
		public DbSet<XpLevel> XpLevels { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Admin>().HasIndex(a => a.Email).IsUnique();
			modelBuilder.Entity<Learner>().HasIndex(l => l.Email).IsUnique();
			modelBuilder.Entity<Manager>().HasIndex(m => m.Email).IsUnique();
			modelBuilder.Entity<TeamMember>().HasIndex(t => t.Email).IsUnique();

			modelBuilder.Entity<PeerEndorsement>()
				.HasIndex(p => new { p.EndorserId, p.EndorseeId, p.SkillId })
				.IsUnique();

			modelBuilder.Entity<UserActivityLog>()
				.HasIndex(u => new { u.UserId, u.CreatedAt });

			modelBuilder.Entity<UserStreak>()
				.HasIndex(s => new { s.UserId, s.UserRole })
				.IsUnique();

			modelBuilder.Entity<XpAction>()
				.HasIndex(a => a.ActionType)
				.IsUnique();

			modelBuilder.Entity<XpLevel>()
				.HasIndex(l => l.Level)
				.IsUnique();

			modelBuilder.Entity<CareerPath>()
				.HasMany(cp => cp.Tracks)
				.WithOne(t => t.CareerPath)
				.HasForeignKey(t => t.CareerPathId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<CareerPathTrack>()
				.HasMany(t => t.CareerPathSkills)
				.WithOne(cps => cps.CareerPathTrack)
				.HasForeignKey(cps => cps.CareerPathTrackId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Organization>()
			    .HasMany(o => o.Managers) 
			    .WithOne(m => m.Organization) 
			    .HasForeignKey(m => m.OrganizationId) 
			    .OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Manager>()
				.HasMany(m => m.TeamMembers)
				.WithOne(t => t.Manager)
				.HasForeignKey(t => t.ManagerId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Organization>()
				.HasMany(o => o.TeamMembers)
				.WithOne(t => t.Organization)
				.HasForeignKey(t => t.OrganizationId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Learner>()
				.HasMany(l => l.LearnerSkills)
				.WithOne(ask => ask.Learner)
				.HasForeignKey(ask => ask.LearnerId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<TeamMember>()
				.HasMany(tm => tm.TeamMemberSkills)
				.WithOne(ask => ask.TeamMember)
				.HasForeignKey(ask => ask.TeamMemberId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Skill>()
				.HasMany(s => s.AssignedSkills)
				.WithOne(ask => ask.Skill)
				.HasForeignKey(ask => ask.SkillId);

			modelBuilder.Entity<Learner>()
				.HasMany(l => l.Badges)
				.WithOne(ab => ab.Learner)
				.HasForeignKey(ab => ab.LearnerID)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<TeamMember>()
				.HasMany(tm => tm.Badges)
				.WithOne(ab => ab.TeamMember)
				.HasForeignKey(ab => ab.TeamMemberId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Learner>()
				.HasMany(l => l.LearnerCareerPaths)
				.WithOne(acp => acp.Learner)
				.HasForeignKey(acp => acp.LearnerId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<TeamMember>()
				.HasMany(tm => tm.CareerPaths)
				.WithOne(acp => acp.TeamMember)
				.HasForeignKey(acp => acp.TeamMemberId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<AssessmentBatch>()
				.HasOne(ab => ab.AssignedSkill)
				.WithMany(ask => ask.AssessmentBatches)
				.HasForeignKey(ab => ab.SkillId);

			modelBuilder.Entity<AssessmentBatch>()
				.HasOne(ab => ab.Learner)
				.WithMany(ab => ab.AssessmentBatches)
				.HasForeignKey(ab => ab.LearnerID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AssessmentBatch>()
				.HasOne(ab => ab.TeamMember)
				.WithMany(ab => ab.AssessmentBatches)
				.HasForeignKey(ab => ab.TeamMemberID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Assessment>()
				.HasOne(a => a.AssessmentBatch)
				.WithMany(ab => ab.Assessments)
				.HasForeignKey(a => a.AssessmentBatchId);

			modelBuilder.Entity<AssessmentOptions>()
				.HasOne(ao => ao.Assessment)
				.WithMany(a => a.AssessmentOptions)
				.HasForeignKey(ao => ao.AssessmentId);

			modelBuilder.Entity<UserResponse>()
				.HasOne(ur => ur.Learner)
				.WithMany(ur => ur.UserResponses)
				.HasForeignKey(ur => ur.LearnerId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<UserResponse>()
				.HasOne(ur => ur.TeamMember)
				.WithMany(ur => ur.UserResponses)
				.HasForeignKey(ur => ur.TeamMemberId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<UserResponse>()
				.HasOne(ur => ur.AssessmentQuestion)
				.WithMany(aq => aq.UserResponses)
				.HasForeignKey(ur => ur.AssessmentQuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserResponse>()
				.HasOne(ur => ur.SelectedOption)
				.WithMany(ur => ur.UserResponses)
				.HasForeignKey(ur => ur.SelectedOptionId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<UserResponse>()
				.HasIndex(ur => new { ur.AssessmentBatchId, ur.AssessmentQuestionId })
				.IsUnique();

			modelBuilder.Entity<AssessmentResult>()
				.HasOne(ar => ar.Learner)
				.WithMany(l => l.AssessmentResults)
				.HasForeignKey(ar => ar.LearnerID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AssessmentResult>()
				.HasOne(ar => ar.TeamMember)
				.WithMany(ar => ar.AssessmentResults)
				.HasForeignKey(ar => ar.TeamMemberID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AssessmentResult>()
				.HasOne(ar => ar.Skill)
				.WithMany(ask => ask.AssessmentResults)
				.HasForeignKey(ar => ar.SkillId);

			modelBuilder.Entity<AssessmentResult>()
				.HasOne(ar => ar.AssessmentBatch)
				.WithOne(ab => ab.AssessmentResult)
				.HasForeignKey<AssessmentResult>(ar => ar.AssessmentBatchId);

			modelBuilder.Entity<ImprovementPlan>()
				.HasOne(ip => ip.AssessmentResult)
				.WithOne(ar => ar.ImprovementPlan)
				.HasForeignKey<ImprovementPlan>(ip => ip.AssessmentResultId)
				.IsRequired(false);

			modelBuilder.Entity<ImprovementPlan>()
				.HasOne(ip => ip.AssignedSkill)
				.WithMany()
				.HasForeignKey(ip => ip.AssignedSkillId)
				.IsRequired(false);

			modelBuilder.Entity<RecommendedResource>()
				.HasOne(ip => ip.ImprovementPlan)
				.WithMany(rr => rr.RecommendedResources)
				.HasForeignKey(ip => ip.ImprovementPlanId);

			modelBuilder.Entity<SkillGap>()
				.HasOne(sg => sg.Learner)
				.WithMany()
				.HasForeignKey(sg => sg.LearnerId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<SkillGap>()
				.HasOne(sg => sg.TeamMember)
				.WithMany()
				.HasForeignKey(sg => sg.TeamMemberId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<SkillGap>()
				.HasOne(sg => sg.Skill)
				.WithMany()
				.HasForeignKey(sg => sg.SkillId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<SkillGap>()
				.HasOne(sg => sg.AssessmentResult)
				.WithMany()
				.HasForeignKey(sg => sg.AssessmentResultId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ImprovementTask>()
				.HasOne(it => it.ImprovementPlan)
				.WithMany(ip => ip.Tasks)
				.HasForeignKey(it => it.ImprovementPlanId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ImprovementTask>()
				.HasOne(it => it.RecommendedResource)
				.WithMany()
				.HasForeignKey(it => it.RecommendedResourceId)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<Admin>().HasData
				(
					new Admin
					{
						Id = new Guid("f4b1f8b4-5b4a-4b4a-8b4a-5b4a4b4a4b4a"),
						FirstName = "Super",
						LastName = "Admin",
						Email = "skillmatrix77@gmail.com",
						UserName = "Superadmin",
						PasswordHash = "$2a$12$U6EhdNjpXotPZ04t54w.ZeNsMONXMidmU1WMPjOdAehb5OylKpZK2",
						Role = Domain.Enum.Roles.SuperAdmin.ToString(),
						DateJoined = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
						PasswordResetToken = default,
						PasswordResetTokenExpiry = default
					}
				);

			// XP Action seed data
			var seedDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			modelBuilder.Entity<XpAction>().HasData(
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000001"), ActionType = "AssessmentCompleted", BaseXp = 50, Description = "Complete an assessment", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000002"), ActionType = "ImprovementTaskCompleted", BaseXp = 20, Description = "Complete an improvement task", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000003"), ActionType = "PeerEndorsed", BaseXp = 10, Description = "Receive a peer endorsement", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000004"), ActionType = "SkillMastered", BaseXp = 100, Description = "Master a skill (reach Expert level)", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000005"), ActionType = "BadgeUnlocked", BaseXp = 30, Description = "Unlock a badge", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000006"), ActionType = "CareerPathCompleted", BaseXp = 200, Description = "Complete a career path", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpAction { Id = new Guid("a1000000-0000-0000-0000-000000000007"), ActionType = "BaselineCompleted", BaseXp = 25, Description = "Complete a baseline assessment", CreatedAt = seedDate, UpdatedAt = seedDate }
			);

			// XP Level seed data (15 levels, quadratic curve matching frontend)
			modelBuilder.Entity<XpLevel>().HasData(
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000001"), Level = 1, MinXp = 0, Title = "Rookie", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000002"), Level = 2, MinXp = 100, Title = "Rookie", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000003"), Level = 3, MinXp = 400, Title = "Apprentice", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000004"), Level = 4, MinXp = 900, Title = "Apprentice", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000005"), Level = 5, MinXp = 1600, Title = "Journeyperson", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000006"), Level = 6, MinXp = 2500, Title = "Journeyperson", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000007"), Level = 7, MinXp = 3600, Title = "Section Chief", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000008"), Level = 8, MinXp = 4900, Title = "Section Chief", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000009"), Level = 9, MinXp = 6400, Title = "Specialist", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000010"), Level = 10, MinXp = 8100, Title = "Specialist", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000011"), Level = 11, MinXp = 10000, Title = "Specialist", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000012"), Level = 12, MinXp = 12100, Title = "Master", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000013"), Level = 13, MinXp = 14400, Title = "Master", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000014"), Level = 14, MinXp = 16900, Title = "Master", CreatedAt = seedDate, UpdatedAt = seedDate },
				new XpLevel { Id = new Guid("b1000000-0000-0000-0000-000000000015"), Level = 15, MinXp = 19600, Title = "Grandmaster", CreatedAt = seedDate, UpdatedAt = seedDate }
			);
		}
	}
}