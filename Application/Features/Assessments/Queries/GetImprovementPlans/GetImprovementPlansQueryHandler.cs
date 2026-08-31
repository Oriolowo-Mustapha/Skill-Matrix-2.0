using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Queries.GetImprovementPlans
{
	public class GetImprovementPlansQueryHandler : IRequestHandler<GetImprovementPlansQuery, List<ImprovementPlanDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetImprovementPlansQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<ImprovementPlanDTO>> Handle(GetImprovementPlansQuery request, CancellationToken cancellationToken)
		{
			var improvementPlans = await _unitOfWork.ImprovementPlans.GetAllAsync();

			if (request.UserId.HasValue && request.UserRole != Roles.Manager.ToString() && request.UserRole != Roles.Admin.ToString())
			{
				var uid = request.UserId.Value;
				var filtered = improvementPlans.Where(p =>
					(p.AssessmentResult != null && (p.AssessmentResult.LearnerID == uid || p.AssessmentResult.TeamMemberID == uid)) ||
					(p.AssignedSkill != null && (p.AssignedSkill.LearnerId == uid || p.AssignedSkill.TeamMemberId == uid))
				).ToList();

				return filtered.Select(p => p.ToDto(p.AssignedSkill?.Name ?? "")).ToList();
			}

			return improvementPlans.Select(p => p.ToDto(p.AssignedSkill?.Name ?? "")).ToList();
		}
	}
}
