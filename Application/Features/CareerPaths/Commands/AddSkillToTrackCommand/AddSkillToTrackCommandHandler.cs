using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.AddSkillToTrackCommand
{
    public class AddSkillToTrackCommandHandler : IRequestHandler<AddSkillToTrackCommand, BaseResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddSkillToTrackCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<Guid>> Handle(AddSkillToTrackCommand request, CancellationToken cancellationToken)
        {
            // Validate track belongs to career path
            var trackExists = await _unitOfWork.CareerPathTracks.ExistsAsync(
                t => t.Id == request.TrackId && t.CareerPathId == request.CareerPathId);
            if (!trackExists)
                throw new NotFoundException($"Track with ID {request.TrackId} not found for CareerPath {request.CareerPathId}.");

            // Validate skill exists
            var skillExists = await _unitOfWork.Skills.ExistsAsync(s => s.Id == request.SkillId);
            if (!skillExists)
                throw new NotFoundException($"Skill with ID {request.SkillId} not found.");

            // Check for duplicate
            var alreadyExists = await _unitOfWork.CareerPathSkills.ExistsAsync(
                cps => cps.CareerPathId == request.CareerPathId &&
                       cps.SkillId == request.SkillId &&
                       cps.CareerPathTrackId == request.TrackId);
            if (alreadyExists)
                throw new ConflictException($"Skill is already assigned to this track.");

            var careerPathSkill = new CareerPathSkill
            {
                CareerPathId = request.CareerPathId,
                SkillId = request.SkillId,
                CareerPathTrackId = request.TrackId,
                TargetLevel = request.TargetLevel
            };

            await _unitOfWork.CareerPathSkills.AddAsync(careerPathSkill);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BaseResponse<Guid>.SuccessResponse(careerPathSkill.Id, "Skill successfully added to track.");
        }
    }
}
