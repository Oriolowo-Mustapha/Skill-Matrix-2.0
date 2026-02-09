using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Extensions
{
    public static class BadgeMappingExtensions
    {
        public static BadgeDTO ToBadgeDTO(this Badge badge)
        {
            if (badge == null)
            {
                return null;
            }

            return new BadgeDTO
            {
                Id = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                IconUrl = badge.IconURL,
                Criteria = badge.Criteria,
                ProficiencyLevel = badge.ProficiencyLevel
            };
        }

        public static List<BadgeDTO> ToBadgeDTOList(this IEnumerable<Badge> badges)
        {
            return badges?.Select(b => b.ToBadgeDTO()).ToList() ?? new List<BadgeDTO>();
        }
    }
}
