using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface IUserService
	{
		Task<UserDTO> GetUserProfileAsync(Guid userId);
		Task<IEnumerable<SkillDTO>> GetAssignedSkillAsync(Guid userId);
		Task<IEnumerable<AssessmentResultSummaryDTO>> GetAssessmentResultHistoryAsync(Guid userId);
	}
}
