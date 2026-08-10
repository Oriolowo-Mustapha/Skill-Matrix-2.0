using MediatR;
using Application.DTOs;

namespace Application.Features.Skills.Commands
{
    public class GenerateAiSkillCatalogCommand : IRequest<BaseResponse<string>>
    {
    }
}
