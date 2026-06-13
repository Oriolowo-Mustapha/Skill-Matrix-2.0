using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand
{
    public class AssignCareerPathToLearnerCommandHandler : IRequestHandler<AssignCareerPathToLearnerCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignCareerPathToLearnerCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(AssignCareerPathToLearnerCommand request, CancellationToken cancellationToken)
        {
            var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.CareerPathId);

            if (careerPath == null)
            {
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");
            }

            var assignedCareerPath = new AssignedCareerPath
            {
                LearnerId = request.LearnerId,
                CareerPathId = request.CareerPathId,
                Title = careerPath.Title,
                Description = careerPath.Description,
                ImageUrl = careerPath.IconURL,
                DateAssigned = DateTime.UtcNow
            };

            await _unitOfWork.AssignedCareerPaths.AddAsync(assignedCareerPath);

            // Auto-assign career path skills (Edge Case: batch assignment to prevent DB round-trips)
            var pathSkills = await _unitOfWork.CareerPathSkills.FindAsync(
                cps => cps.CareerPathId == request.CareerPathId,
                cps => cps.Skill
            );

            var existingAssigned = await _unitOfWork.AssignedSkills.FindAsync(
                s => s.LearnerId == request.LearnerId
            );
            var existingSkillDict = existingAssigned.ToDictionary(s => s.SkillId);
            var newAssignedSkills = new List<AssignedSkill>();

            foreach (var pathSkill in pathSkills)
            {
                if (!existingSkillDict.TryGetValue(pathSkill.SkillId, out var existingSkill))
                {
                    var newAssigned = new AssignedSkill
                    {
                        LearnerId = request.LearnerId,
                        SkillId = pathSkill.SkillId,
                        Name = pathSkill.Skill.Name,
                        Category = pathSkill.Skill.Category,
                        ProficiencyLevel = pathSkill.TargetLevel, // Map to the required level of the career path
                        DateAssigned = DateTime.UtcNow
                    };
                    newAssignedSkills.Add(newAssigned);
                }
                else
                {
                    // Edge Case: Conflicting Proficiency Targets (upgrade existing target level if the career path requires it)
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

            return assignedCareerPath.Id;
        }
    }
}
