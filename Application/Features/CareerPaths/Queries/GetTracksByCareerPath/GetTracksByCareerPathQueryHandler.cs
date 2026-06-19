using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Queries.GetTracksByCareerPath
{
    public class GetTracksByCareerPathQueryHandler : IRequestHandler<GetTracksByCareerPathQuery, List<CareerPathTrackDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTracksByCareerPathQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CareerPathTrackDTO>> Handle(GetTracksByCareerPathQuery request, CancellationToken cancellationToken)
        {
            var careerPathExists = await _unitOfWork.CareerPaths.ExistsAsync(cp => cp.Id == request.CareerPathId);
            if (!careerPathExists)
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");

            // Find tracks and eager load their associated skills & skill entities
            var tracks = await _unitOfWork.CareerPathTracks.FindAsync(
                t => t.CareerPathId == request.CareerPathId,
                t => t.CareerPathSkills);

            // We need to load the Skill details for each CareerPathSkill since they are lazy loaded/not in Include above
            // Let's load CareerPathSkills with Skill for the career path and map them
            var pathSkills = await _unitOfWork.CareerPathSkills.FindAsync(
                cps => cps.CareerPathId == request.CareerPathId,
                cps => cps.Skill
            );

            // Build a dictionary of CareerPathSkillId -> SkillDTO or Skill mapping
            var skillsDict = pathSkills
                .Where(cps => cps.CareerPathTrackId != null)
                .GroupBy(cps => cps.CareerPathTrackId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(cps => new SkillDTO
                    {
                        Id = cps.Skill.Id,
                        Name = cps.Skill.Name,
                        Category = cps.Skill.Category
                    }).ToList()
                );

            var trackDtos = new List<CareerPathTrackDTO>();
            foreach (var track in tracks)
            {
                var dto = track.ToDto();
                if (skillsDict.TryGetValue(track.Id, out var trackSkills))
                {
                    dto.Skills = trackSkills;
                }
                trackDtos.Add(dto);
            }

            return trackDtos;
        }
    }
}
