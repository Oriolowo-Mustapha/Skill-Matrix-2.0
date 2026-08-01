using Application.DTOs;
using Application.Interfaces.Repository;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Queries.GetAssessmentHistory
{
	public class GetAssessmentHistoryQueryHandler : IRequestHandler<GetAssessmentHistoryQuery, BaseResponse<List<AssessmentResultDTO>>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssessmentHistoryQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<List<AssessmentResultDTO>>> Handle(GetAssessmentHistoryQuery request, CancellationToken cancellationToken)
		{
			var isLearner = request.UserRole == Roles.Learner.ToString();
			
			var results = await _unitOfWork.AssessmentResults.FindAsync(
				r => isLearner ? r.LearnerID == request.UserId : r.TeamMemberID == request.UserId,
				r => r.Skill
			);

			var dtoList = results
				.OrderByDescending(r => r.DateCreated)
				.Select(r => new AssessmentResultDTO
				{
					Id = r.Id,
					SkillName = r.Skill?.Name ?? "Skill Assessment",
					Score = r.Score,
					NoOfCorrectAnswers = r.NoOfCorrectAnswers,
					NoOfWrongAnswers = r.NoOfWrongAnswers,
					TotalQuestions = r.TotalQuestions,
					ProficiencyLevel = r.ProficiencyLevel.ToString(),
					DateCompleted = r.DateCreated,
					Passed = r.Score >= 60,
					PassingScore = 60
				})
				.ToList();

			return BaseResponse<List<AssessmentResultDTO>>.SuccessResponse(dtoList, "Assessment history retrieved successfully.");
		}
	}
}
