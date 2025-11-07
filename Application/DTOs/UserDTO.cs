namespace Application.DTOs
{
	public record UserDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string UserName { get; set; }
		public string Role { get; set; }
		public string? ProfilePicUrl { get; set; }
	}

	public record LoginRequestDTO
	{
		public string? UserName { get; set; }
		public string Password { get; set; }
		public string? Email { get; set; }
	}

	public record LoginResponseDTO
	{
		public string Token { get; set; }
		public UserDTO User { get; set; }
	}
}
