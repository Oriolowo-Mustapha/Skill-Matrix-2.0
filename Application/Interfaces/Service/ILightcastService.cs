using Application.DTOs;

namespace Application.Interfaces.Service
{
    public interface ILightcastService
    {
        Task<List<LightcastSkillDto>> GetSkillsAsync(int limit, string taxonomyVersion);
    }
}
