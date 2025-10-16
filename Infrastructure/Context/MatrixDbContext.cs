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


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Admin>().HasIndex(a => a.Email).IsUnique();
			modelBuilder.Entity<Learner>().HasIndex(l => l.Email).IsUnique();
			modelBuilder.Entity<Manager>().HasIndex(m => m.Email).IsUnique();
			modelBuilder.Entity<TeamMember>().HasIndex(t => t.Email).IsUnique();

			modelBuilder.Entity<Organization>()
				.HasOne(o => o.Manager)
				.WithOne(m => m.Organization)
				.HasForeignKey<Manager>(m => m.OrganizationId)
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
				.OnDelete(DeleteBehavior.NoAction);

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

			modelBuilder.Entity<Admin>().HasData
				(
					new Admin
					{
						FirstName = "Super",
						LastName = "Admin",
						Email = "superAdmin@gmail.com",
						UserName = "Superadmin",
						PasswordHash = HashPassword("SkillMatrix@SuperAdmin"),
						Role = Domain.Enum.Roles.SuperAdmin
					}

				);
		}
		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}