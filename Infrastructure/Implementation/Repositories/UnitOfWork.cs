using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Repositories;
using Infrastructure.Implementation.Servicies;

namespace Infrastructure.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly MatrixDbContext _context;

		// Backing fields for the repositories
		private IAssessmentBatchRepository? _assessmentBatches;
		private IGenericRepository<AssessmentResult>? _assessmentResults;
		private IAssignedSkillRepository? _assignedSkills;
		private IImprovementPlanRepository? _improvementPlans;
		private IUserResponseRepository? _userResponseRepository;
		private ISkillRepository? _skills;
		private ILearnerRepository? _learners;
		private ITeamMemberRepository? _teamMembers;
		private IManagerRepository? _managers;
		private IGenericRepository<Organization>? _organizations;
		private IGenericRepository<Badge>? _badges;
		private IGenericRepository<CareerPath>? _careerPaths;

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

		public IGenericRepository<Organization> Organizations =>
			_organizations ??= new GenericRepository<Organization>(_context);

		public IGenericRepository<Badge> Badges =>
			_badges ??= new GenericRepository<Badge>(_context);

		public IGenericRepository<CareerPath> CareerPaths =>
			_careerPaths ??= new GenericRepository<CareerPath>(_context);

		public IUserResponseRepository UserResponses =>
			_userResponseRepository ??= new UserResponseRepository(_context);

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