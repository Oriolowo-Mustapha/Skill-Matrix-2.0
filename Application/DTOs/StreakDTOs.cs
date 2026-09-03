namespace Application.DTOs
{
	public record StreakDTO
	{
		public int CurrentStreak { get; init; }
		public int LongestStreak { get; init; }
		public DateTime? LastActivityDate { get; init; }
		public int FreezeTokens { get; init; }
		public DateTime? StreakStartDate { get; init; }
		public bool IsBroken { get; init; }
		public DateTime? BrokenDate { get; init; }
	}

	public record XpActionDTO
	{
		public string ActionType { get; init; }
		public int BaseXp { get; init; }
		public string? Description { get; init; }
	}

	public record XpLevelDTO
	{
		public int Level { get; init; }
		public int MinXp { get; init; }
		public string Title { get; init; }
	}

	public record RepairStreakResponseDTO
	{
		public bool Success { get; init; }
		public string Message { get; init; }
		public int NewStreak { get; init; }
		public int XpCost { get; init; }
	}

	public record XpConfigDTO
	{
		public List<XpActionDTO> Actions { get; init; }
		public List<XpLevelDTO> Levels { get; init; }
	}
}
