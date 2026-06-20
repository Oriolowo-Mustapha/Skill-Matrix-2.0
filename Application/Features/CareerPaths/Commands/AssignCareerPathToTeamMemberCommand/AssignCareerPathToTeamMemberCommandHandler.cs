using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
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

        public AssignCareerPathToTeamMemberCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<Guid>> Handle(AssignCareerPathToTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.CareerPathId);

            if (careerPath == null)
            {
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");
            }

            // Validate TrackId if provided
            if (request.TrackId.HasValue)
            {
                var trackExists = await _unitOfWork.CareerPathTracks.ExistsAsync(
                    t => t.Id == request.TrackId.Value && t.CareerPathId == request.CareerPathId);
                if (!trackExists)
                {
                    throw new NotFoundException($"Track with ID {request.TrackId.Value} not found for CareerPath {request.CareerPathId}.");
                }
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
                        ProficiencyLevel = pathSkill.TargetLevel,
                        DateAssigned = DateTime.UtcNow
                    };
                    newAssignedSkills.Add(newAssigned);
                }
                else
                {
                    if (pathSkill.TargetLevel > existingSkill.ProficiencyLevel)
                    {
                        existingSkill.ProficiencyLevel = pathSkill.TargetLevel;
                        await _unitOfWork.AssignedSkills.UpdateAsync(existingSkill);
                    }
                }
            }

            if (newAssignedSkills.Any())
            {
                await _unitOfWork.AssignedSkills.AddRangeAsync(newAssignedSkills);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BaseResponse<Guid>.SuccessResponse(assignedCareerPath.Id, "Career path successfully assigned to team member.");
        }
    }
}
