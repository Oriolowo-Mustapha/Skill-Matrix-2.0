using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathTrackCommand
{
    public class CreateCareerPathTrackCommandHandler : IRequestHandler<CreateCareerPathTrackCommand, BaseResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCareerPathTrackCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<Guid>> Handle(CreateCareerPathTrackCommand request, CancellationToken cancellationToken)
        {
            var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.CareerPathId);
            if (careerPath == null)
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");

            var track = new CareerPathTrack
            {
                CareerPathId = request.CareerPathId,
                Name = request.Name,
                Description = request.Description,
                IconUrl = request.IconUrl
            };

            await _unitOfWork.CareerPathTracks.AddAsync(track);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BaseResponse<Guid>
            {
                Data = track.Id,
                Message = "CareerPath Created Successfully. ",
                Success = true
            };
        }
    }
}
