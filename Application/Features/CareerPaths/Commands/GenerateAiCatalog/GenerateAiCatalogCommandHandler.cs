using Application.DTOs.Ai;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.GenerateAiCatalog
{
    public class GenerateAiCatalogCommandHandler : IRequestHandler<GenerateAiCatalogCommand, CatalogGenerationResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiService _aiService;

        public GenerateAiCatalogCommandHandler(
            IUnitOfWork unitOfWork,
            IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<CatalogGenerationResultDto> Handle(GenerateAiCatalogCommand request, CancellationToken cancellationToken)
        {
            var existingSkills = await _unitOfWork.Skills.GetAllAsync();
            var skillList = existingSkills.ToList();

            if (!skillList.Any())
            {
                return new CatalogGenerationResultDto
                {
                    CreatedPathsCount = 0,
                    CreatedTracksCount = 0,
                    MappedSkillsCount = 0,
                    Message = "No skills found in database. Please sync or create skills first."
                };
            }

            var skillNames = skillList.Select(s => s.Name).ToList();
            var aiPaths = await _aiService.GenerateGlobalCatalogAsync(skillNames);

            int createdPaths = 0;
            int createdTracks = 0;
            int mappedSkills = 0;

            var allExistingPaths = (await _unitOfWork.CareerPaths.GetAllAsync()).ToList();
            var allExistingTracks = (await _unitOfWork.CareerPathTracks.GetAllAsync()).ToList();
            var mappedPathSkillsList = (await _unitOfWork.CareerPathSkills.GetAllAsync()).ToList();

            foreach (var aiPath in aiPaths)
            {
                if (string.IsNullOrWhiteSpace(aiPath.Title)) continue;

                var existingPath = allExistingPaths.FirstOrDefault(p => p.Title.Equals(aiPath.Title.Trim(), StringComparison.OrdinalIgnoreCase));
                CareerPath pathEntity;

                if (existingPath != null)
                {
                    pathEntity = existingPath;
                }
                else
                {
                    pathEntity = new CareerPath
                    {
                        Title = aiPath.Title.Trim(),
                        Description = aiPath.Description?.Trim() ?? "AI generated career path."
                    };
                    await _unitOfWork.CareerPaths.AddAsync(pathEntity);
                    allExistingPaths.Add(pathEntity);
                    createdPaths++;
                }

                foreach (var aiTrack in aiPath.Tracks)
                {
                    if (string.IsNullOrWhiteSpace(aiTrack.Name)) continue;

                    var existingTrack = allExistingTracks.FirstOrDefault(t => 
                        t.CareerPathId == pathEntity.Id && 
                        t.Name.Equals(aiTrack.Name.Trim(), StringComparison.OrdinalIgnoreCase));

                    CareerPathTrack trackEntity;

                    if (existingTrack != null)
                    {
                        trackEntity = existingTrack;
                    }
                    else
                    {
                        trackEntity = new CareerPathTrack
                        {
                            CareerPathId = pathEntity.Id,
                            Name = aiTrack.Name.Trim(),
                            Description = aiTrack.Description?.Trim() ?? "AI generated track."
                        };
                        await _unitOfWork.CareerPathTracks.AddAsync(trackEntity);
                        allExistingTracks.Add(trackEntity);
                        createdTracks++;
                    }

                    foreach (var aiSkill in aiTrack.Skills)
                    {
                        if (string.IsNullOrWhiteSpace(aiSkill.SkillName)) continue;

                        var matchedSkill = skillList.FirstOrDefault(s => s.Name.Equals(aiSkill.SkillName.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (matchedSkill == null)
                        {
                            bool reqCoding = await _aiService.ClassifySkillRequiresCodingAsync(aiSkill.SkillName);
                            matchedSkill = new Skill
                            {
                                Name = aiSkill.SkillName.Trim(),
                                Category = "AI Catalog Generated",
                                Source = "AI Catalog",
                                RequiresCoding = reqCoding
                            };
                            await _unitOfWork.Skills.AddAsync(matchedSkill);
                            skillList.Add(matchedSkill);
                        }

                        var alreadyMapped = mappedPathSkillsList.Any(cps => 
                            cps.CareerPathId == pathEntity.Id && 
                            cps.CareerPathTrackId == trackEntity.Id && 
                            cps.SkillId == matchedSkill.Id);

                        if (!alreadyMapped)
                        {
                            var trackSkill = new CareerPathSkill
                            {
                                CareerPathId = pathEntity.Id,
                                CareerPathTrackId = trackEntity.Id,
                                SkillId = matchedSkill.Id,
                                TargetLevel = (ProficiencyLevel)Math.Clamp(aiSkill.TargetLevel, 0, 4)
                            };
                            await _unitOfWork.CareerPathSkills.AddAsync(trackSkill);
                            mappedPathSkillsList.Add(trackSkill);
                            mappedSkills++;
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CatalogGenerationResultDto
            {
                CreatedPathsCount = createdPaths,
                CreatedTracksCount = createdTracks,
                MappedSkillsCount = mappedSkills,
                Message = $"Successfully generated catalog! Created {createdPaths} career paths, {createdTracks} tracks, and mapped {mappedSkills} skills."
            };
        }
    }
}
