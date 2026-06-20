using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Skills.Commands.SelfAssignSkill
{
	public class SelfAssignSkillCommandHandler : IRequestHandler<SelfAssignSkillCommand, BaseResponse<bool>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public SelfAssignSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<bool>> Handle(SelfAssignSkillCommand request, CancellationToken cancellationToken)
		{
			var masterSkill = await _unitOfWork.Skills.GetByIdAsync(request.SkillId);
			if (masterSkill == null)
			{
				throw new NotFoundException("Skill", request.SkillId);
			}

			var existingAssignment = await _unitOfWork.AssignedSkills.GetByUserAndSkillId(request.UserId, request.SkillId);
			if (existingAssignment != null)
			{
				throw new ConflictException("You already have this skill assigned to your profile.");
			}

			var newAssignment = new AssignedSkill
			{
				SkillId = request.SkillId,
				LearnerId = request.UserId,
				TeamMemberId = null,
				DateAssigned = DateTime.UtcNow,
				ProficiencyLevel = ProficiencyLevel.Novice,
				Name = masterSkill.Name,
				Category = masterSkill.Category
			};

			await _unitOfWork.AssignedSkills.AddAsync(newAssignment);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<bool>.SuccessResponse(true, "Skill successfully added to your profile.");
		}
	}
}
