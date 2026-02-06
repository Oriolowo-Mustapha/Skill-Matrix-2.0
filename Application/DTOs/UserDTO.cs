using Domain.Enum;

namespace Application.DTOs
{
	public record UserDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string? ProfilePicUrl { get; set; }
	}

	public record LoginRequestDTO
	{
		public string? UserName { get; set; }
		public string Password { get; set; } = string.Empty;
		public string? Email { get; set; }
	}

	public record LoginResponseDTO
	{
		public string Token { get; set; } = string.Empty;
		public UserDTO User { get; set; } = null!;
	}

	public record RegisterLearnerRequestDTO
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Role { get; set; } = Roles.Learner.ToString();
		public string? ProfilePicUrl { get; set; }
		public string PasswordHash { get; set; } = string.Empty;
	}

	public record RegisterManagerRequestDTO
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public string PasswordHash { get; set; } = string.Empty;
	}

	public record UpdateUserRequestDTO
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
	}
}
