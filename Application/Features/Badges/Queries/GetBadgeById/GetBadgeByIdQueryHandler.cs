using Application.Exceptions;
using Application.Features.Badges.Queries.GetBadgeById;
using Application.Interfaces.Repository;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs; // Assuming BadgeDTO exists
using Application.Extensions; // Using custom mapping extensions

namespace Application.Features.Badges.Queries.GetBadgeById
{
    public class GetBadgeByIdQueryHandler : IRequestHandler<GetBadgeByIdQuery, BadgeDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBadgeByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BadgeDTO> Handle(GetBadgeByIdQuery request, CancellationToken cancellationToken)
        {
            var badge = await _unitOfWork.Badges.GetByIdAsync(request.Id);

            if (badge == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Badge), request.Id);
            }

            return badge.ToBadgeDTO();
        }
    }
}
