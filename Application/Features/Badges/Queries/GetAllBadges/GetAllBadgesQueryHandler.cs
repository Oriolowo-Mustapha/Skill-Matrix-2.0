using Application.Features.Badges.Queries.GetAllBadges;
using Application.Interfaces.Repository;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs; // Assuming BadgeDTO exists
using Application.Extensions; // Using custom mapping extensions

namespace Application.Features.Badges.Queries.GetAllBadges
{
    public class GetAllBadgesQueryHandler : IRequestHandler<GetAllBadgesQuery, List<BadgeDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllBadgesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BadgeDTO>> Handle(GetAllBadgesQuery request, CancellationToken cancellationToken)
        {
            var badges = await _unitOfWork.Badges.GetAllAsync();
            return badges.ToBadgeDTOList();
        }
    }
}
