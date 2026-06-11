using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
    public record RegisterOrganizationRequestDTO
    {
        // Organization Details
        public string OrganizationName { get; set; } = string.Empty;
        public IFormFile? OrganizationProfilePicture { get; set; }
        public string OrganizationDescription { get; set; } = string.Empty;

        // Manager Details
        public string ManagerFirstName { get; set; } = string.Empty;
        public string ManagerLastName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string ManagerUserName { get; set; } = string.Empty;
        public string ManagerPassword { get; set; } = string.Empty;
    }

    public record OrganizationDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateJoined { get; set; }
    }
}
