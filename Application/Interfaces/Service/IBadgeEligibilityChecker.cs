using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Service
{
    public interface IBadgeEligibilityChecker
    {
        Task<bool> EvaluateEligibilityAsync(Guid userId, string expectedProficiency, string customCriteria);
    }
}
