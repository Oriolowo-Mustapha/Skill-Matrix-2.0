using Application.DTOs;
using Domain.Entities;

namespace Application.Extensions
{
	public static class CareerPathMappingExtensions
	{
		public static CareerPathDTO ToDto(this CareerPath careerPath)
		{
			return new CareerPathDTO
			{
				Id = careerPath.Id,
				Title = careerPath.Title,
				Description = careerPath.Description,
				IconURL = careerPath.IconURL,
				DateAdded = careerPath.DateAdded,
				Skills = careerPath.CareerPathSkills?
					.Select(cps => new SkillDTO
					{
						Id = cps.Skill.Id,
						Name = cps.Skill.Name,
						Category = cps.Skill.Category
					}).ToList() ?? new()
			};
		}

		public static List<CareerPathDTO> ToDtoList(this IEnumerable<CareerPath> careerPaths)
		{
			return careerPaths.Select(cp => cp.ToDto()).ToList();
		}
	}
}