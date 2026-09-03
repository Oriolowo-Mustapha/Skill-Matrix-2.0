namespace Domain.Entities
{
	public class XpLevel : BaseEntity
	{
		public int Level { get; set; }
		public int MinXp { get; set; }
		public string Title { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
