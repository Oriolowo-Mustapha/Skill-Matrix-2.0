using Application.Exceptions;
using Application.Features.Badges.Queries.GetAssignedBadgesForLearner;
using Application.Interfaces.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs; // Assuming BadgeDTO exists
using Application.Extensions; // Using custom mapping extensions
using Domain.Entities;

namespace Application.Features.Badges.Queries.GetAssignedBadgesForLearner
{
    public class GetAssignedBadgesForLearnerQueryHandler : IRequestHandler<GetAssignedBadgesForLearnerQuery, List<BadgeDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignedBadgesForLearnerQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BadgeDTO>> Handle(GetAssignedBadgesForLearnerQuery request, CancellationToken cancellationToken)
        {
            var learner = await _unitOfWork.Learners.GetByIdAsync(request.LearnerId);
            if (learner == null)
            {
                throw new NotFoundException(nameof(Learner), request.LearnerId);
            }

            var assignedBadges = (await _unitOfWork.AssignedBadges.GetAllAsync())
                                 .Where(ab => ab.LearnerID == request.LearnerId)
                                 .ToList();

            var badgeDTOs = new List<BadgeDTO>();
            foreach (var assignedBadge in assignedBadges)
            {
                var badge = await _unitOfWork.Badges.GetByIdAsync(assignedBadge.BadgeId);
                if (badge != null)
                {
                    badgeDTOs.Add(badge.ToBadgeDTO());
                }
            }

            return badgeDTOs;
        }
    }
}
