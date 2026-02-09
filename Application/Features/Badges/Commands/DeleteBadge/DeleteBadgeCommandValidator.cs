using Application.Features.Badges.Commands.DeleteBadge;
using Application.Interfaces.Repository;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities; // Needed for the Badge entity in ExistsAsync

namespace Application.Features.Badges.Commands.DeleteBadge
{
	public class DeleteBadgeCommandValidator : AbstractValidator<DeleteBadgeCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteBadgeCommandValidator(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

			RuleFor(p => p.Id)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MustAsync(BadgeExists).WithMessage("Badge with this ID does not exist.");
		}

		private async Task<bool> BadgeExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Badges.ExistsAsync(b => b.Id == id);
		}
	}
}
