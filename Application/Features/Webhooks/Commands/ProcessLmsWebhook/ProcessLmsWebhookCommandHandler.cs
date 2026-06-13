using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Webhooks.Commands.ProcessLmsWebhook
{
	public class ProcessLmsWebhookCommandHandler : IRequestHandler<ProcessLmsWebhookCommand, BaseResponse<bool>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public ProcessLmsWebhookCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<bool>> Handle(ProcessLmsWebhookCommand request, CancellationToken cancellationToken)
		{
			var payload = request.Payload;

			// Determine if user is a Learner or TeamMember
			var learner = await _unitOfWork.Learners.GetByEmailAsync(payload.UserEmail);
			var teamMember = await _unitOfWork.TeamMembers.GetByEmailAsync(payload.UserEmail);

			if (learner == null && teamMember == null)
			{
				// User not found in our system, silently ignore or throw
				throw new NotFoundException($"User with email {payload.UserEmail} not found.");
			}

			// Find the skill by name (ignoring case)
			var allSkills = await _unitOfWork.Skills.GetAllAsync();
			var targetSkill = allSkills.FirstOrDefault(s => s.Name.Equals(payload.SkillName, StringComparison.OrdinalIgnoreCase));

			if (targetSkill == null)
			{
				throw new NotFoundException($"Skill '{payload.SkillName}' not found in the matrix.");
			}

			// Find if user already has this skill assigned
			var allAssignedSkills = await _unitOfWork.AssignedSkills.GetAllAsync();
			var assignedSkill = allAssignedSkills.FirstOrDefault(a => 
				a.SkillId == targetSkill.Id && 
				(learner != null ? a.LearnerId == learner.Id : a.TeamMemberId == teamMember.Id));

			if (assignedSkill == null)
			{
				assignedSkill = new AssignedSkill
				{
					Name = targetSkill.Name,
					Category = targetSkill.Category,
					SkillId = targetSkill.Id,
					LearnerId = learner?.Id,
					TeamMemberId = teamMember?.Id,
					ProficiencyLevel = ProficiencyLevel.Intermediate,
					DateAssigned = DateTime.UtcNow
				};
				await _unitOfWork.AssignedSkills.AddAsync(assignedSkill);
			}
			else
			{
				assignedSkill.ProficiencyLevel = ProficiencyLevel.Intermediate;
				await _unitOfWork.AssignedSkills.UpdateAsync(assignedSkill);
			}

			if (learner != null)
			{
				learner.TotalPoints += 20; // 20 points for an LMS course
				await _unitOfWork.Learners.UpdateAsync(learner);
			}
			else if (teamMember != null)
			{
				teamMember.TotalPoints += 20;
				await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<bool>.SuccessResponse(true, $"Webhook processed successfully. Proficiency updated for {payload.UserEmail}.");
		}
	}
}
