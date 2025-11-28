using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface IUnitOfWork : IDisposable
	{
		IAssessmentBatchRepository AssessmentBatches { get; }
		IGenericRepository<AssessmentResult> AssessmentResults { get; }
		IAssignedSkillRepository AssignedSkills { get; }
		IImprovementPlanRepository ImprovementPlans { get; }
		ISkillRepository Skills { get; }
		IUserResponseRepository UserResponses { get; }

		ILearnerRepository Learners { get; }
		ITeamMemberRepository TeamMembers { get; }
		IGenericRepository<Manager> Managers { get; }
		IGenericRepository<Organization> Organizations { get; }

		IGenericRepository<Badge> Badges { get; }
		IGenericRepository<CareerPath> CareerPaths { get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}