using Application.DTOs;
﻿using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.Assessments.Commands.TeamManagement
{
	public class AssignSkillCommandHandler : IRequestHandler<AssignSkillCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public AssignSkillCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
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

			var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(request.ManagerId);

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

			var subject = $"New Skill Assigned: {FetchSkillById.Name}";
			var body = $"""
				Dear {FetchTeamMemberById.UserName},

				Your manager, {manager?.UserName ?? "your manager"}, has assigned you a new skill on Skill Matrix 2.0.

				Skill Details:
				- Name: {FetchSkillById.Name}
				- Category: {FetchSkillById.Category}
				- Proficiency Level: Novice
				- Date Assigned: {newAssignment.DateAssigned:MMMM dd, yyyy}

				Please log in to your account to view your updated skill set and begin any related assessments.

				Best regards,
				The Skill Matrix 2.0 Team
				""";

			await _emailService.SendEmailAsync(FetchTeamMemberById.Email, subject, body);

			return true;
		}
	}
}