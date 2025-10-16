namespace Domain.Entities
{
	public class Badge : BaseEntity
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string IconURL { get; set; }
		public DateTime DateAdded { get; set; }
	}
}
