using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface ISkillCatalogService
	{
		Task<IEnumerable<SkillDTO>> GetAllSkillsAsync();
		Task<SkillDTO> CreateSkillAsync(SkillDTO skillDTO);
	}
}
