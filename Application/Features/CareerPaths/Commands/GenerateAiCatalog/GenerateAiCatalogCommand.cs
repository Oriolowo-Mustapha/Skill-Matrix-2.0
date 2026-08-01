using Application.DTOs.Ai;
using MediatR;

namespace Application.Features.CareerPaths.Commands.GenerateAiCatalog
{
    public class GenerateAiCatalogCommand : IRequest<CatalogGenerationResultDto>
    {
    }
}
