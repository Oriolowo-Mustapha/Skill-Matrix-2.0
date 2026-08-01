using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Skills.Queries.GetAssignedSkills
{
    public class GetAssignedSkillsQueryHandler : IRequestHandler<GetAssignedSkillsQuery, BaseResponse<List<SkillDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignedSkillsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<List<SkillDTO>>> Handle(GetAssignedSkillsQuery request, CancellationToken cancellationToken)
        {
            var assignedSkills = await _unitOfWork.AssignedSkills.FindAsync(a => a.LearnerId == request.UserId || a.TeamMemberId == request.UserId);
            
            var skillDTOs = assignedSkills.Select(a => a.ToDTO()).ToList();
            
            return BaseResponse<List<SkillDTO>>.SuccessResponse(skillDTOs, "Assigned skills retrieved successfully");
        }
    }
}
