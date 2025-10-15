using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
	public class MatrixDbContext : DbContext
	{
		public MatrixDbContext(DbContextOptions<MatrixDbContext> options) : base(options) { }

		public DbSet<Admin> Admin { get; set; }
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
		public DbSet<Team_Members> TeamMembers { get; set; }
		public DbSet<TeamMemberBadge> TeamMemberBadges { get; set; }
		public DbSet<TeamMemberCareerPath> TeamMemberCareerPaths { get; set; }
		public DbSet<UserBadge> UserBadges { get; set; }
		public DbSet<UserCareerPath> UserCareerPaths { get; set; }
		public DbSet<UserResponse> UserResponses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Admin>()
				.HasIndex(a => a.Email).IsUnique();
			modelBuilder.Entity<Learner>().HasIndex(l => l.Email).IsUnique();
			modelBuilder.Entity<Manager>().HasIndex(m => m.Email).IsUnique();

			modelBuilder.Entity<AssessmentResult>()
				.HasOne(a => a.Admin)
				.WithMany(ass => ass.AssessmentResults)
				.HasForeignKey(a => a.AdminID)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
