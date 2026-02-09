using Application.Exceptions;
using Application.Features.Badges.Commands.DeleteBadge;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Badges.Commands.DeleteBadge
{
    public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBadgeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            var badge = await _unitOfWork.Badges.GetByIdAsync(request.Id);

            if (badge == null)
            {
                throw new NotFoundException(nameof(Badge), request.Id);
            }

            await _unitOfWork.Badges.DeleteAsync(badge);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
