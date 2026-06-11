using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Assessments.Commands.ImprovementPlans
{
	public class GenerateImprovementPlanCommandHandler : IRequestHandler<GenerateImprovementPlanCommand, BaseResponse<ImprovementPlanDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public GenerateImprovementPlanCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<BaseResponse<ImprovementPlanDTO>> Handle(GenerateImprovementPlanCommand request, CancellationToken cancellationToken)
		{
			var existingPlan = await _unitOfWork.ImprovementPlans.GetByAssessmentResultIdAsync(request.AssessmentResultId);

			if (existingPlan != null)
			{
				return BaseResponse<ImprovementPlanDTO>.SuccessResponse(existingPlan.ToDto());
			}

			var assessmentResult = await _unitOfWork.AssessmentResults.GetByIdAsync(request.AssessmentResultId);

			if (assessmentResult == null)
			{
				throw new NotFoundException(nameof(AssessmentResult), request.AssessmentResultId);
			}

			bool isOwner = false;
			if (request.UserRole == Roles.Learner.ToString())
			{
				isOwner = assessmentResult.LearnerID == request.UserId;
			}
			else if (request.UserRole == Roles.Team_Members.ToString() || request.UserRole == "TeamMember")
			{
				isOwner = assessmentResult.TeamMemberID == request.UserId;
			}

			if (!isOwner)
			{
				throw new ForbiddenException("You are not authorized to access this assessment result.");
			}

			var newPlan = await _aiService.GenerateImprovementPlanAsync(assessmentResult);

			newPlan.AssessmentResultId = request.AssessmentResultId;

			await _unitOfWork.ImprovementPlans.AddAsync(newPlan);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<ImprovementPlanDTO>.SuccessResponse(newPlan.ToDto());
		}
	}
}
