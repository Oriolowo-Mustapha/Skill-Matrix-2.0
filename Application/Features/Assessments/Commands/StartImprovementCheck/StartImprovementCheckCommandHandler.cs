using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Commands.StartImprovementCheck
{
	public class StartImprovementCheckCommandHandler : IRequestHandler<StartImprovementCheckCommand, BaseResponse<StartAssessmentResponseDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public StartImprovementCheckCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<BaseResponse<StartAssessmentResponseDTO>> Handle(StartImprovementCheckCommand request, CancellationToken cancellationToken)
		{
			var assignedSkill = await _unitOfWork.AssignedSkills.GetByUserAndSkillId(request.UserId, request.SkillId);
			if (assignedSkill == null)
			{
				throw new NotFoundException("Assigned Skill", request.SkillId);
			}

			// Get the parent Skill to check RequiresCoding
			var skill = await _unitOfWork.Skills.GetByIdAsync(assignedSkill.SkillId);
			if (skill == null)
			{
				throw new NotFoundException("Skill", assignedSkill.SkillId);
			}

			// 1. Find the active SkillGap for this skill and concept
			var gaps = await _unitOfWork.SkillGaps.FindAsync(
				g => g.SkillId == assignedSkill.Id && // SkillId on SkillGap points to AssignedSkill ID
				     g.Concept == request.Concept &&
				     g.Status == "Active" &&
				     (request.UserRole == Roles.Learner.ToString() ? g.LearnerId == request.UserId : g.TeamMemberId == request.UserId)
			);
			var gap = gaps.FirstOrDefault();

			if (gap == null)
			{
				throw new BadRequestException($"No active gap found for the concept '{request.Concept}' on this skill.");
			}

			// 2. Fetch the latest ImprovementPlan for this gap's AssessmentResult
			var plans = await _unitOfWork.ImprovementPlans.FindAsync(
				p => p.AssessmentResultId == gap.AssessmentResultId,
				p => p.Tasks
			);
			var plan = plans.FirstOrDefault();

			if (plan == null)
			{
				throw new BadRequestException("No improvement plan found for the assessment result.");
			}

			// 3. Verify that all tasks for this concept in the plan are completed
			var conceptTasks = plan.Tasks.Where(t => t.Concept.Equals(request.Concept, StringComparison.OrdinalIgnoreCase)).ToList();
			if (conceptTasks.Any(t => t.Status != "Completed"))
			{
				throw new BadRequestException($"You must complete all study tasks for the concept '{request.Concept}' before taking the Improvement Check.");
			}

			// 4. Cooldown period validation: check if there's any ImprovementCheck batch for this concept in the last 12 hours
			var recentBatches = await _unitOfWork.AssessmentBatches.FindAsync(
				b => b.SkillId == assignedSkill.SkillId &&
				     b.BatchType == "ImprovementCheck" &&
				     b.ConceptFocus == request.Concept &&
				     (request.UserRole == Roles.Learner.ToString() ? b.LearnerID == request.UserId : b.TeamMemberID == request.UserId) &&
				     b.DateCreated > DateTime.UtcNow.AddHours(-12)
			);

			if (recentBatches.Any())
			{
				throw new BadRequestException("Cooldown active. You must wait 12 hours between attempts for this Concept Re-Assessment.");
			}

			// 5. Determine targeted micro-assessment parameters
			// MCQ @ 2m each; Coding @ 10m each.
			// Target: exactly 3 questions specifically on this concept.
			int timeLimitMinutes = skill.RequiresCoding ? 14 : 6; // coding: 2 MCQ (4m) + 1 Coding (10m) = 14m; non-coding: 3 MCQ (6m)

			var questions = await _aiService.GenerateTargetedQuestionsAsync(
				assignedSkill.Name,
				assignedSkill.ProficiencyLevel.ToString(),
				request.Concept,
				3, // 3 questions
				skill.RequiresCoding
			);

			var startedAt = DateTime.UtcNow;
			var expiresAt = startedAt.AddMinutes(timeLimitMinutes);

			var batch = new AssessmentBatch
			{
				SkillId = assignedSkill.SkillId,
				AssessmentStatus = AssessmentStatus.InProgress,
				DateCreated = startedAt,
				StartedAt = startedAt,
				ExpiresAt = expiresAt,
				TimeLimitMinutes = timeLimitMinutes,
				LastActiveQuestionIndex = 0,
				BatchType = "ImprovementCheck",
				ConceptFocus = request.Concept,
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

			return BaseResponse<StartAssessmentResponseDTO>.SuccessResponse(batch.ToDTO(), "Improvement check started successfully.");
		}
	}
}
