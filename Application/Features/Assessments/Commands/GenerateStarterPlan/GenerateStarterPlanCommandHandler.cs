using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Commands.GenerateStarterPlan
{
	public class GenerateStarterPlanCommandHandler : IRequestHandler<GenerateStarterPlanCommand, BaseResponse<ImprovementPlanDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public GenerateStarterPlanCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<BaseResponse<ImprovementPlanDTO>> Handle(GenerateStarterPlanCommand request, CancellationToken cancellationToken)
		{
			var assignedSkill = await _unitOfWork.AssignedSkills.GetByUserAndSkillId(request.UserId, request.Dto.AssignedSkillId);
			if (assignedSkill == null)
			{
				throw new NotFoundException("Assigned Skill", request.Dto.AssignedSkillId);
			}

			// Check if a starter plan already exists for this assigned skill
			var existingPlans = await _unitOfWork.ImprovementPlans.FindAsync(p => p.AssignedSkillId == assignedSkill.Id && p.IsStarterPlan);
			var existingPlan = existingPlans.FirstOrDefault();

			if (existingPlan != null)
			{
				var existingDto = existingPlan.ToDTO();
				return BaseResponse<ImprovementPlanDTO>.SuccessResponse(existingDto, "Existing starter learning plan retrieved.");
			}

			// Generate new starter plan via AI
			var plan = await _aiService.GenerateStarterPlanAsync(assignedSkill.Name, assignedSkill.Category);
			plan.AssignedSkillId = assignedSkill.Id;

			await _unitOfWork.ImprovementPlans.AddAsync(plan);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var dto = plan.ToDTO();
			return BaseResponse<ImprovementPlanDTO>.SuccessResponse(dto, "Starter learning roadmap generated successfully.");
		}
	}
}
