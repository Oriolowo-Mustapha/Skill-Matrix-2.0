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
	public class StartAssessmentCommandHandler : IRequestHandler<StartAssessmentCommand, BaseResponse<StartAssessmentResponseDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public StartAssessmentCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<BaseResponse<StartAssessmentResponseDTO>> Handle(StartAssessmentCommand request, CancellationToken cancellationToken)
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

			// Apply ClaimedLevel if provided by the user in the Experience Gate placement path
			if (!string.IsNullOrWhiteSpace(request.Dto.ClaimedLevel))
			{
				var claimedStr = request.Dto.ClaimedLevel.Trim();
				if (claimedStr.Equals("Beginner", StringComparison.OrdinalIgnoreCase)) claimedStr = "Begineer";

				if (Enum.TryParse<ProficiencyLevel>(claimedStr, true, out var parsedLevel))
				{
					assignedSkill.ProficiencyLevel = parsedLevel;
					await _unitOfWork.AssignedSkills.UpdateAsync(assignedSkill);
					await _unitOfWork.SaveChangesAsync(cancellationToken);
				}
			}

			var package = await _aiService.GenerateAssessmentPackageAsync(
				assignedSkill.Name,
				assignedSkill.ProficiencyLevel.ToString(),
				mcqCount,
				codingCount,
				skill.RequiresCoding
			);

			var startedAt = DateTime.UtcNow;
			int effectiveTimeLimit = package.TimeLimitMinutes > 0 ? package.TimeLimitMinutes : timeLimitMinutes;
			var expiresAt = startedAt.AddMinutes(effectiveTimeLimit);

			string batchType = !assignedSkill.IsBaselineAssessed ? "Baseline" : "Progression";

			var batch = new AssessmentBatch
			{
				SkillId = assignedSkill.Id,
				AssessmentStatus = AssessmentStatus.InProgress,
				DateCreated = startedAt,
				StartedAt = startedAt,
				ExpiresAt = expiresAt,
				TimeLimitMinutes = effectiveTimeLimit,
				LastActiveQuestionIndex = 0,
				BatchType = batchType,
				Assessments = package.Questions
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

			string? warningMessage = null;
			var recentResults = await _unitOfWork.AssessmentResults.FindAsync(
				r => r.SkillId == assignedSkill.Id &&
				     r.DateCreated > DateTime.UtcNow.AddDays(-1) &&
				     (request.UserRole == Roles.Learner.ToString() ? r.LearnerID == request.UserId : r.TeamMemberID == request.UserId)
			);
			var actuallyPassed = recentResults.Any(r => r.Score >= GetPassingThreshold(r.ProficiencyLevel));
			if (actuallyPassed)
			{
				warningMessage = "You recently unlocked this level. We recommend completing at least one preparation task before taking this assessment, but you may bypass this and proceed if desired.";
			}

			var dto = batch.ToDTO();
			dto.WarningMessage = warningMessage;
			return BaseResponse<StartAssessmentResponseDTO>.SuccessResponse(dto, "Assessment started successfully.");
		}

		private static int GetPassingThreshold(ProficiencyLevel level)
		{
			return level switch
			{
				ProficiencyLevel.Novice => 50,
				ProficiencyLevel.Begineer => 60,
				ProficiencyLevel.Intermediate => 70,
				ProficiencyLevel.Proficient => 80,
				ProficiencyLevel.Expert => 90,
				_ => 70
			};
		}
	}
}