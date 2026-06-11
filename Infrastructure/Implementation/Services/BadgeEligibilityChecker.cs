using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Enum;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Implementation.Services
{
    public class BadgeEligibilityChecker : IBadgeEligibilityChecker
    {
        private readonly IUnitOfWork _unitOfWork;

        public BadgeEligibilityChecker(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> EvaluateEligibilityAsync(Guid userId, string expectedProficiency, string customCriteria)
        {
            // 1. Evaluate Proficiency Level
            if (!string.IsNullOrWhiteSpace(expectedProficiency))
            {
                if (Enum.TryParse<ProficiencyLevel>(expectedProficiency, true, out var requiredProficiency))
                {
                    // Check if learner or team member has an assigned skill at or above this proficiency.
                    var userSkills = await _unitOfWork.AssignedSkills.FindAsync(
                        s => (s.LearnerId == userId || s.TeamMemberId == userId) &&
                             s.ProficiencyLevel >= requiredProficiency);

                    if (userSkills == null || userSkills.Count == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    // Invalid proficiency string in badge configuration
                    return false;
                }
            }

            // 2. Evaluate Custom Criteria (if provided)
            if (!string.IsNullOrWhiteSpace(customCriteria))
            {
                // Future expansion: Deserialize JSON criteria string to evaluate specific assessment scores.
                // Example:
                // var criteria = JsonSerializer.Deserialize<BadgeCriteria>(customCriteria);
                // var pastAssessments = await _unitOfWork.AssessmentResults.FindAsync(a => a.LearnerID == userId || a.TeamMemberID == userId);
                // if (pastAssessments.Max(a => a.Score) < criteria.MinScore) return false;
            }

            return true;
        }
    }
}
