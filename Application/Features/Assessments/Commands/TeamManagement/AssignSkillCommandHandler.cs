using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Assessments.Commands.TeamManagement
{
	public class AssignSkillCommandHandler : IRequestHandler<AssignSkillCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public AssignSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<bool> Handle(AssignSkillCommand request, CancellationToken cancellationToken)
		{
			var FetchTeamMemberById = await _unitOfWork.TeamMembers.GetByIdAsync(request.TeamMemberId);
			if (FetchTeamMemberById == null)
			{
				throw new NotFoundException("TeamMember", request.TeamMemberId);
			}
			if (FetchTeamMemberById.ManagerId != request.ManagerId)
			{
				throw new ForbiddenException("This employee does not belong to your team");
			}

			var FetchSkillById = await _unitOfWork.Skills.GetByIdAsync(request.SkillId);
			if (FetchSkillById == null)
			{
				throw new NotFoundException("Skill", request.SkillId);
			}

			var FetchAssignedSkillByUserSkillId = await _unitOfWork.AssignedSkills.GetByUserAndSkillId(request.TeamMemberId, request.SkillId);
			if (FetchAssignedSkillByUserSkillId != null)
			{
				throw new ConflictException("User already has this skill assigned");
			}

			var newAssignment = new AssignedSkill
			{
				SkillId = request.SkillId,
				TeamMemberId = request.TeamMemberId,
				LearnerId = null,
				DateAssigned = DateTime.UtcNow,
				ProficiencyLevel = Domain.Enum.ProficiencyLevel.Novice,
				Name = FetchSkillById.Name,
				Category = FetchSkillById.Category
			};

			await _unitOfWork.AssignedSkills.AddAsync(newAssignment);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return true;
		}
	}
}
