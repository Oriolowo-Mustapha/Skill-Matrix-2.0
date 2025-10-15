namespace Domain.Entities
{
	public class Organization : BaseEntity
	{
		public string Name { get; set; }
		public string ProfilePictureUrl { get; set; }
		public Guid ManagerId { get; set; }
		public Manager Manager { get; set; }
		public List<Team_Members> Team_Members { get; set; }
	}
}
