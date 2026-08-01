using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Commands.StartTrackBaseline
{
    public class StartTrackBaselineCommandHandler : IRequestHandler<StartTrackBaselineCommand, BaseResponse<List<StartAssessmentResponseDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiService _aiService;

        public StartTrackBaselineCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<List<StartAssessmentResponseDTO>>> Handle(StartTrackBaselineCommand request, CancellationToken cancellationToken)
        {
            var track = await _unitOfWork.CareerPathTracks.GetByIdAsync(request.Dto.CareerPathTrackId);
            if (track == null)
                throw new NotFoundException("CareerPathTrack", request.Dto.CareerPathTrackId);

            var trackSkills = await _unitOfWork.CareerPathSkills.FindAsync(
                cs => cs.CareerPathTrackId == request.Dto.CareerPathTrackId,
                cs => cs.Skill
            );

            if (!trackSkills.Any())
                throw new BadRequestException("This track has no associated skills to assess.");

            var responseDtos = new List<StartAssessmentResponseDTO>();

            foreach (var trackSkill in trackSkills)
            {
                // Find existing assigned skill or create a new one
                AssignedSkill? assignedSkill;
                if (request.UserRole == Roles.Learner.ToString())
                {
                    var existing = await _unitOfWork.AssignedSkills.FindAsync(
                        s => s.SkillId == trackSkill.SkillId && s.LearnerId == request.UserId);
                    assignedSkill = existing.FirstOrDefault();
                }
                else if (request.UserRole == Roles.Team_Members.ToString() || request.UserRole == "TeamMember")
                {
                    var existing = await _unitOfWork.AssignedSkills.FindAsync(
                        s => s.SkillId == trackSkill.SkillId && s.TeamMemberId == request.UserId);
                    assignedSkill = existing.FirstOrDefault();
                }
                else
                {
                    throw new BadRequestException("Only Learners and Team Members can take assessments.");
                }

                if (assignedSkill == null)
                {
                    assignedSkill = new AssignedSkill
                    {
                        SkillId = trackSkill.SkillId,
                        Name = trackSkill.Skill.Name,
                        Category = trackSkill.Skill.Category,
                        ProficiencyLevel = ProficiencyLevel.Novice,
                        DateAssigned = DateTime.UtcNow
                    };

                    if (request.UserRole == Roles.Learner.ToString())
                        assignedSkill.LearnerId = request.UserId;
                    else
                        assignedSkill.TeamMemberId = request.UserId;

                    await _unitOfWork.AssignedSkills.AddAsync(assignedSkill);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Determine difficulty: min of target level and user's declared level
                var testDifficultyLevel = (ProficiencyLevel)Math.Min((int)request.Dto.DeclaredProficiencyLevel, (int)trackSkill.TargetLevel);

                // Auto-classify RequiresCoding if needed
                var skill = trackSkill.Skill;
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

                int mcqCount = 2;
                int codingCount = skill.RequiresCoding ? 1 : 0;
                int timeLimitMinutes = (mcqCount * 2) + (codingCount * 10);

                var questions = await _aiService.GenerateAssessmentQuestionsAsync(
                    skill.Name,
                    testDifficultyLevel.ToString(),
                    mcqCount,
                    codingCount,
                    skill.RequiresCoding
                );

                var batch = new AssessmentBatch
                {
                    SkillId = assignedSkill.Id,
                    AssessmentStatus = AssessmentStatus.InProgress,
                    DateCreated = DateTime.UtcNow,
                    StartedAt = DateTime.UtcNow,
                    TimeLimitMinutes = timeLimitMinutes,
                    BatchType = "Baseline",
                    Assessments = questions.ToList()
                };

                if (request.UserRole == Roles.Learner.ToString())
                    batch.LearnerID = request.UserId;
                else
                    batch.TeamMemberID = request.UserId;

                await _unitOfWork.AssessmentBatches.AddAsync(batch);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var dto = batch.ToDTO();
                responseDtos.Add(dto);
            }

            return BaseResponse<List<StartAssessmentResponseDTO>>.SuccessResponse(responseDtos, "Track Baseline Assessment generated successfully.");
        }
    }
}
