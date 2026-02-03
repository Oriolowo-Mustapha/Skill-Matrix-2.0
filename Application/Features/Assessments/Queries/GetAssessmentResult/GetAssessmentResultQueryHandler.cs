using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Queries.GetAssessmentResult
{
	public class GetAssessmentResultQueryHandler : IRequestHandler<GetAssessmentResultQuery, AssessmentResultDTO>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssessmentResultQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<AssessmentResultDTO> Handle(GetAssessmentResultQuery request, CancellationToken cancellationToken)
		{
			var assessmentResult = await _unitOfWork.AssessmentResults.GetByIdAsync(request.ResultId);

			if (assessmentResult == null || (assessmentResult.LearnerID != request.UserId && assessmentResult.TeamMemberID != request.UserId))
			{
				throw new NotFoundException("Assessment Result", request.ResultId);
			}
			var assignedSkill = await _unitOfWork.AssignedSkills.GetByIdAsync(assessmentResult.SkillId);
			if (assignedSkill == null)
			{
				throw new NotFoundException("Assigned Skill", assessmentResult.SkillId);
			}


			return assessmentResult.ToDTO(assignedSkill.Name);
		}
	}
}
