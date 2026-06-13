using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Assessments.Commands.StartAssessment
{
	public class StartAssessmentCommandHandler : IRequestHandler<StartAssessmentCommand, StartAssessmentResponseDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public StartAssessmentCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<StartAssessmentResponseDTO> Handle(StartAssessmentCommand request, CancellationToken cancellationToken)
		{

			var assignedSkill = await _unitOfWork.AssignedSkills.GetByUserAndSkillId(request.UserId, request.Dto.AssignedSkillId);

			if (assignedSkill == null)
			{
				throw new NotFoundException("Assigned Skill", request.Dto.AssignedSkillId);
			}

			// Get the parent Skill to check RequiresCoding
			var skill = await _unitOfWork.Skills.GetByIdAsync(assignedSkill.SkillId);
			if (skill == null)
			{
				throw new NotFoundException("Skill", assignedSkill.SkillId);
			}

			// If RequiresCoding has never been classified, auto-classify via AI
			if (!skill.RequiresCoding && skill.Source != "System")
			{
				var isCoding = await _aiService.ClassifySkillRequiresCodingAsync(skill.Name);
				if (isCoding)
				{
					skill.RequiresCoding = true;
					await _unitOfWork.Skills.UpdateAsync(skill);
					await _unitOfWork.SaveChangesAsync(cancellationToken);
				}
			}

			// Determine question counts and timer based on RequiresCoding
			int mcqCount;
			int codingCount;
			int timeLimitMinutes;

			if (skill.RequiresCoding)
			{
				mcqCount = 10;
				codingCount = 5;
				timeLimitMinutes = (mcqCount * 2) + (codingCount * 10); // 70 minutes
			}
			else
			{
				mcqCount = 10;
				codingCount = 5; // These become scenario-based MCQs for non-coding skills
				timeLimitMinutes = (mcqCount * 2) + (codingCount * 3); // 35 minutes
			}

			var questions = await _aiService.GenerateAssessmentQuestionsAsync(
				assignedSkill.Name,
				assignedSkill.ProficiencyLevel.ToString(),
				mcqCount,
				codingCount,
				skill.RequiresCoding
			);

			var batch = new AssessmentBatch
			{
				SkillId = assignedSkill.SkillId,
				AssessmentStatus = AssessmentStatus.InProgress,
				DateCreated = DateTime.UtcNow,
				StartedAt = DateTime.UtcNow,
				TimeLimitMinutes = timeLimitMinutes,
				Assessments = questions.ToList()
			};

			if (request.UserRole == Roles.Learner.ToString())
			{
				batch.LearnerID = request.UserId;
			}
			else if (request.UserRole == Roles.Team_Members.ToString() || request.UserRole == "TeamMember")
			{
				batch.TeamMemberID = request.UserId;
			}
			else
			{
				throw new BadRequestException("Only Learners and Team Members can take assessments.");
			}


			await _unitOfWork.AssessmentBatches.AddAsync(batch);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return batch.ToDTO();
		}
	}
}