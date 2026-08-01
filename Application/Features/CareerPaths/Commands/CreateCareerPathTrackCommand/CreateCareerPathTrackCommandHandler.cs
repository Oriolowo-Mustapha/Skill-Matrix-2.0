using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
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
        private readonly IPhotoService _photoService;
        private readonly IAiService _aiService;

        public CreateCareerPathTrackCommandHandler(IUnitOfWork unitOfWork, IPhotoService photoService, IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _aiService = aiService;
        }

        public async Task<BaseResponse<Guid>> Handle(CreateCareerPathTrackCommand request, CancellationToken cancellationToken)
        {
            var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.CareerPathId);
            if (careerPath == null)
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");

            string? iconUrl = null;
            if (request.Icon != null)
            {
                iconUrl = await _photoService.AddPhotoAsync(request.Icon);
            }

            var track = new CareerPathTrack
            {
                CareerPathId = request.CareerPathId,
                Name = request.Name,
                Description = request.Description,
                IconUrl = iconUrl
            };

            await _unitOfWork.CareerPathTracks.AddAsync(track);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                var generatedSkills = await _aiService.GenerateSkillsForTrackAsync(careerPath.Title, track.Name);
                
                foreach (var generatedSkill in generatedSkills)
                {
                    // Check if skill already exists
                    var existingSkill = await _unitOfWork.Skills.GetByNameAsync(generatedSkill.SkillName.Trim());
                    if (existingSkill == null)
                    {
                        existingSkill = new Skill
                        {
                            Name = generatedSkill.SkillName.Trim(),
                            Category = "AI Generated",
                            Source = "AI",
                            DateAdded = DateTime.UtcNow
                        };
                        await _unitOfWork.Skills.AddAsync(existingSkill);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    var careerPathSkill = new CareerPathSkill
                    {
                        CareerPathId = careerPath.Id,
                        CareerPathTrackId = track.Id,
                        SkillId = existingSkill.Id,
                        TargetLevel = generatedSkill.TargetLevel
                    };

                    await _unitOfWork.CareerPathSkills.AddAsync(careerPathSkill);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // We shouldn't fail the track creation if AI fails, just log it (or it can throw if strictly required)
                // For now we'll throw, or return success with a warning message. 
                // Let's just let it throw or handle it. 
                // Let's wrap in a nice error if needed.
                throw new Exception($"Track created but failed to generate skills via AI: {ex.Message}");
            }

            return new BaseResponse<Guid>
            {
                Data = track.Id,
                Message = "CareerPath Created Successfully. ",
                Success = true
            };
        }
    }
}
