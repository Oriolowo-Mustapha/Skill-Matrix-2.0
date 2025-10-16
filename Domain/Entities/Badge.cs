namespace Domain.Entities
{
	public class Badge : BaseEntity
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string IconURL { get; set; } = string.Empty;
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
	}
}
