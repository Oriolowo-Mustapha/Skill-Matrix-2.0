using MediatR;
using Application.DTOs;

namespace Application.Features.Skills.Commands
{
    public class SyncLightcastSkillsCommand : IRequest<BaseResponse<string>>
    {
        public int Limit { get; set; } = 500;
        public string TaxonomyVersion { get; set; } = "latest";
    }
}
