namespace Domain.Entities
{
	public class Badge : BaseEntity
	{
		public string Name { get; set; } = string.Empty; // Renamed from Title
		public string Description { get; set; } = string.Empty;
		public string IconURL { get; set; } = string.Empty;
		public string Criteria { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
	}
}