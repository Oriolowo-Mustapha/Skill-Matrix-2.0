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

		IManagerRepository ManagerRepository { get; }
		IAdminRepository Admins { get; }
		ILearnerRepository Learners { get; }
		ITeamMemberRepository TeamMembers { get; }
		IGenericRepository<Organization> Organizations { get; }

		IGenericRepository<Badge> Badges { get; }
		IGenericRepository<AssignedBadge> AssignedBadges { get; }
		IGenericRepository<CareerPath> CareerPaths { get; }
        IGenericRepository<CareerPathSkill> CareerPathSkills { get; }
        IGenericRepository<CareerPathTrack> CareerPathTracks { get; }
        IGenericRepository<AssignedCareerPath> AssignedCareerPaths { get; }
		IGenericRepository<PeerEndorsement> PeerEndorsements { get; }
		IGenericRepository<SkillGap> SkillGaps { get; }
		IGenericRepository<ImprovementTask> ImprovementTasks { get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}