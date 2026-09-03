namespace Domain.Entities
{
	public class XpAction : BaseEntity
	{
		public string ActionType { get; set; } = string.Empty;
		public int BaseXp { get; set; }
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
