using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
	public record RegisterTeamMemberRequestDTO
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public IFormFile? ProfilePicUrl { get; set; }
	}

	public record TeamMemberDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string? ProfilePicUrl { get; set; }
		public Guid OrganizationId { get; set; }
		public Guid ManagerId { get; set; }
		public List<Guid> AssignedCareerPathIds { get; set; } = new();
	}
}
