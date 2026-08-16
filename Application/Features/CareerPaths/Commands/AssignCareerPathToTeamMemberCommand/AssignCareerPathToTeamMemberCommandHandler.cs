using Application.DTOs;
using Application.Exceptions;
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

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand
{
    public class AssignCareerPathToTeamMemberCommandHandler : IRequestHandler<AssignCareerPathToTeamMemberCommand, BaseResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AssignCareerPathToTeamMemberCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<BaseResponse<Guid>> Handle(AssignCareerPathToTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.TeamMemberId);
            if (teamMember == null)
            {
                throw new NotFoundException($"TeamMember with ID {request.TeamMemberId} not found.");
            }

            var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.CareerPathId);
            if (careerPath == null)
            {
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");
            }

            string? trackName = null;
            // Validate TrackId if provided
            if (request.TrackId.HasValue)
            {
                var track = await _unitOfWork.CareerPathTracks.GetByIdAsync(request.TrackId.Value);
                if (track == null || track.CareerPathId != request.CareerPathId)
                {
                    throw new NotFoundException($"Track with ID {request.TrackId.Value} not found for CareerPath {request.CareerPathId}.");
                }
                trackName = track.Name;
            }

            var alreadyAssigned = await _unitOfWork.AssignedCareerPaths.ExistsAsync(
                acp => acp.TeamMemberId == request.TeamMemberId && acp.CareerPathId == request.CareerPathId
            );
            
            if (alreadyAssigned)
            {
                throw new ConflictException("This team member is already assigned to this career path.");
            }

            var assignedCareerPath = new AssignedCareerPath
            {
                TeamMemberId = request.TeamMemberId,
                CareerPathId = request.CareerPathId,
                CareerPathTrackId = request.TrackId,
                Title = careerPath.Title,
                Description = careerPath.Description,
                ImageUrl = careerPath.IconURL,
                DateAssigned = DateTime.UtcNow
            };

            await _unitOfWork.AssignedCareerPaths.AddAsync(assignedCareerPath);

            // Auto-assign career path skills: Core skills (TrackId == null) + selected Track's skills
            var pathSkills = await _unitOfWork.CareerPathSkills.FindAsync(
                cps => cps.CareerPathId == request.CareerPathId &&
                       (cps.CareerPathTrackId == null || cps.CareerPathTrackId == request.TrackId),
                cps => cps.Skill
            );

            var existingAssigned = await _unitOfWork.AssignedSkills.FindAsync(
                s => s.TeamMemberId == request.TeamMemberId
            );
            var existingSkillDict = existingAssigned.ToDictionary(s => s.SkillId);
            var newAssignedSkills = new List<AssignedSkill>();

            foreach (var pathSkill in pathSkills)
            {
                if (!existingSkillDict.TryGetValue(pathSkill.SkillId, out var existingSkill))
                {
                    var newAssigned = new AssignedSkill
                    {
                        TeamMemberId = request.TeamMemberId,
                        SkillId = pathSkill.SkillId,
                        Name = pathSkill.Skill.Name,
                        Category = pathSkill.Skill.Category,
                        ProficiencyLevel = ProficiencyLevel.Novice,
                        TargetProficiencyLevel = pathSkill.TargetLevel,
                        DateAssigned = DateTime.UtcNow
                    };
                    newAssignedSkills.Add(newAssigned);
                }
                else
                {
                    if (pathSkill.TargetLevel > existingSkill.TargetProficiencyLevel)
                    {
                        existingSkill.TargetProficiencyLevel = pathSkill.TargetLevel;
                        await _unitOfWork.AssignedSkills.UpdateAsync(existingSkill);
                    }
                }
            }

            if (newAssignedSkills.Any())
            {
                await _unitOfWork.AssignedSkills.AddRangeAsync(newAssignedSkills);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send Email Notification
            string trackDetail = trackName != null ? $" ({trackName} Track)" : "";
            string subject = $"New Career Path Assigned: {careerPath.Title}";
            string body = $@"Hello {teamMember.FirstName},

You have been assigned to the '{careerPath.Title}' career path{trackDetail}.

Log in to your Skill Matrix dashboard to view your path, see assigned skills, and check your learning progress.

Best regards,
Skill Matrix Team";

            try
            {
                await _emailService.SendEmailAsync(teamMember.Email, subject, body);
            }
            catch
            {
                // Prevent email failures from rolling back database transactions
            }

            return BaseResponse<Guid>.SuccessResponse(assignedCareerPath.Id, "Career path successfully assigned to team member.");
        }
    }
}
