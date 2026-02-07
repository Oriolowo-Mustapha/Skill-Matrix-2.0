using Application.DTOs;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Queries.GetSkills
{
	public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, List<SkillDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetSkillsQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<SkillDTO>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
		{
			var skills = await _unitOfWork.Skills.GetAllAsync();
			return skills.ToSkillDTOList();
		}
	}
}
