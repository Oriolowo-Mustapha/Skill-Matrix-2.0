namespace Domain.Entities
{
	public class UserStreak : BaseEntity
	{
		public Guid UserId { get; set; }
		public string UserRole { get; set; } = string.Empty;
		public int CurrentStreak { get; set; } = 0;
		public int LongestStreak { get; set; } = 0;
		public int PreviousStreakCount { get; set; } = 0;
		public DateTime? LastActivityDate { get; set; }
		public DateTime? StreakStartDate { get; set; }
		public int FreezeTokens { get; set; } = 0;
		public DateTime? LastFreezeUsedDate { get; set; }
		public DateTime? BrokenDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
