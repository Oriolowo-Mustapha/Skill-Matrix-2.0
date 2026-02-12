namespace Application.DTOs
{
	public record ForgotPasswordRequestDTO
	{
		public string Email { get; set; } = string.Empty;
	}
}