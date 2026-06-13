using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Queries.GetSkills
{
	public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, BaseResponse<List<SkillDTO>>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetSkillsQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<List<SkillDTO>>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
		{
			var skills = await _unitOfWork.Skills.GetAllAsync();
			return BaseResponse<List<SkillDTO>>.SuccessResponse(skills.ToSkillDTOList(), "Skills retrieved successfully");
		}
	}
}
