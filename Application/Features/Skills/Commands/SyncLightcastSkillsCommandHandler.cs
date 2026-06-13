using Application.DTOs;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Features.Skills.Commands
{
    public class SyncLightcastSkillsCommandHandler : IRequestHandler<SyncLightcastSkillsCommand, BaseResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILightcastService _lightcastService;

        public SyncLightcastSkillsCommandHandler(IUnitOfWork unitOfWork, ILightcastService lightcastService)
        {
            _unitOfWork = unitOfWork;
            _lightcastService = lightcastService;
        }

        public async Task<BaseResponse<string>> Handle(SyncLightcastSkillsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var lightcastSkills = await _lightcastService.GetSkillsAsync(request.Limit, request.TaxonomyVersion);

                if (lightcastSkills == null || !lightcastSkills.Any())
                {
                    return new BaseResponse<string>
                    {
                        Success = true,
                        Message = "No skills retrieved from Lightcast.",
                        Data = null
                    };
                }

                var existingSkills = await _unitOfWork.Skills.GetAllAsync();
                int addedCount = 0;

                foreach (var ls in lightcastSkills)
                {
                    // Check if skill exists by ExternalId or Name to prevent duplicates
                    var exists = existingSkills.Any(s => s.ExternalId == ls.Id || s.Name.Equals(ls.Name, StringComparison.OrdinalIgnoreCase));

                    if (!exists)
                    {
                        var newSkill = new Skill
                        {
                            Id = Guid.NewGuid(),
                            ExternalId = ls.Id,
                            Name = ls.Name,
                            Category = ls.Type?.Name ?? "Uncategorized",
                            Source = "Lightcast",
                            IsCustomized = false,
                            DateAdded = DateTime.UtcNow
                        };

                        await _unitOfWork.Skills.AddAsync(newSkill);
                        addedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return new BaseResponse<string>
                {
                    Success = true,
                    Message = $"Successfully synced {addedCount} new skills from Lightcast.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>
                {
                    Success = false,
                    Message = $"An error occurred while syncing skills: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
