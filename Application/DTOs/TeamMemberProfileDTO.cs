using System;
using System.Collections.Generic;

namespace Application.DTOs
{
	public class TeamMemberProfileDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public List<SkillDTO> AssignedSkills { get; set; } = new();
		public List<CareerPathDTO> AssignedPaths { get; set; } = new();
	}
}
