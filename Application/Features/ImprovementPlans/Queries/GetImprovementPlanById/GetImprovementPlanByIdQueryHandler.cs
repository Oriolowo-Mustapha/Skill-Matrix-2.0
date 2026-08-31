using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Domain.Enum;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ImprovementPlans.Queries.GetImprovementPlanById
{
	public class GetImprovementPlanByIdQueryHandler : IRequestHandler<GetImprovementPlanByIdQuery, BaseResponse<ImprovementPlanDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetImprovementPlanByIdQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<ImprovementPlanDTO>> Handle(GetImprovementPlanByIdQuery request, CancellationToken cancellationToken)
		{
			var plan = await _unitOfWork.ImprovementPlans.GetPlanWIthResoursesAsync(request.PlanId);
			if (plan == null)
			{
				throw new NotFoundException("Improvement Plan", request.PlanId);
			}

			// Validate ownership if attached to an assessment result or assigned skill
			if (plan.AssessmentResult != null)
			{
				bool isOwner = plan.AssessmentResult.LearnerID == request.UserId || plan.AssessmentResult.TeamMemberID == request.UserId;
				bool isManager = request.UserRole == Roles.Manager.ToString() || request.UserRole == Roles.Admin.ToString();
				if (!isOwner && !isManager)
				{
					throw new UnauthorizedAccessException("You are not authorized to view this improvement plan.");
				}
			}

			string skillName = plan.AssignedSkill?.Name ?? "";
			if (string.IsNullOrWhiteSpace(skillName) && plan.AssessmentResultId.HasValue)
			{
				var result = await _unitOfWork.AssessmentResults.GetByIdAsync(plan.AssessmentResultId.Value);
				if (result != null && result.SkillId != Guid.Empty)
				{
					var assignedSkill = await _unitOfWork.AssignedSkills.GetByIdAsync(result.SkillId);
					skillName = assignedSkill?.Name ?? "";
				}
			}

			var dto = plan.ToDto(skillName);
			return BaseResponse<ImprovementPlanDTO>.SuccessResponse(dto, "Improvement plan retrieved successfully.");
		}
	}
}
