namespace Application.DTOs
{
	public class LeaderboardEntryDTO
	{
		public Guid UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public int TotalPoints { get; set; }
		public string? ProfilePictureUrl { get; set; }
	}
}
