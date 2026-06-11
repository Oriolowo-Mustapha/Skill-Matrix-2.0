using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Servicies;

namespace Infrastructure.Implementation.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly MatrixDbContext _context;

		private IAssessmentBatchRepository? _assessmentBatches;
		private IGenericRepository<AssessmentResult>? _assessmentResults;
		private IAssignedSkillRepository? _assignedSkills;
		private IImprovementPlanRepository? _improvementPlans;
		private IUserResponseRepository? _userResponseRepository;
		private ISkillRepository? _skills;
		private ILearnerRepository? _learners;
		private ITeamMemberRepository? _teamMembers;
		private IManagerRepository? _managers;
		private IAdminRepository? _admins;
		private IGenericRepository<Organization>? _organizations;
		private IGenericRepository<Badge>? _badges;
		private IGenericRepository<CareerPath>? _careerPaths;
		private IGenericRepository<AssignedBadge>? _assignedBadge;
		private IGenericRepository<AssignedCareerPath>? _assignedCareerPaths;
		private IGenericRepository<CareerPathSkill>? _careerPathSkills;

		public UnitOfWork(MatrixDbContext context)
		{
			_context = context;
		}

		public IAssessmentBatchRepository AssessmentBatches =>
			_assessmentBatches ??= new AssessmentBatchRepository(_context);

		public IGenericRepository<AssessmentResult> AssessmentResults =>
			_assessmentResults ??= new GenericRepository<AssessmentResult>(_context);

		public IAssignedSkillRepository AssignedSkills =>
			_assignedSkills ??= new AssignedSkillRepository(_context);

		public IImprovementPlanRepository ImprovementPlans =>
			_improvementPlans ??= new ImprovementPlanRepository(_context);

		public ISkillRepository Skills =>
			_skills ??= new SkillRepository(_context);

		public ILearnerRepository Learners =>
			_learners ??= new LearnerRepository(_context);

		public ITeamMemberRepository TeamMembers =>
			_teamMembers ??= new TeamMemberRepository(_context);

		public IManagerRepository ManagerRepository =>
			_managers ??= new ManagerRepository(_context);

		public IAdminRepository Admins =>
			_admins ??= new AdminRepository(_context);

		public IGenericRepository<Organization> Organizations =>
			_organizations ??= new GenericRepository<Organization>(_context);

		public IGenericRepository<Badge> Badges =>
			_badges ??= new GenericRepository<Badge>(_context);

		public IGenericRepository<CareerPath> CareerPaths =>
			_careerPaths ??= new GenericRepository<CareerPath>(_context);

		public IGenericRepository<AssignedCareerPath> AssignedCareerPaths =>
			_assignedCareerPaths ??= new GenericRepository<AssignedCareerPath>(_context);

		public IGenericRepository<CareerPathSkill> CareerPathSkills =>
			_careerPathSkills ??= new GenericRepository<CareerPathSkill>(_context);

		public IUserResponseRepository UserResponses =>
			_userResponseRepository ??= new UserResponseRepository(_context);

		public IGenericRepository<AssignedBadge> AssignedBadges =>
			_assignedBadge ??= new GenericRepository<AssignedBadge>(_context);
		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return await _context.SaveChangesAsync(cancellationToken);
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}