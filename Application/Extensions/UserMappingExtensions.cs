using Application.DTOs;
using Domain.Entities;

namespace Application.Extensions
{
	public static class UserMappingExtensions
	{
		public static UserDTO ToDto(this Learner learner)
		{
			return new UserDTO
			{
				Id = learner.Id,
				Email = learner.Email,
				FirstName = learner.FirstName,
				LastName = learner.LastName,
				Role = learner.Role.ToString(),
				UserName = learner.UserName,
				ProfilePicUrl = learner.ProfilePictureUrl
			};
		}

		public static UserDTO ToDto(this TeamMember teamMember)
		{
			return new UserDTO
			{
				Id = teamMember.Id,
				Email = teamMember.Email,
				FirstName = teamMember.FirstName,
				LastName = teamMember.LastName,
				Role = teamMember.Role.ToString(),
				UserName = teamMember.UserName,
				ProfilePicUrl = teamMember.ProfilePictureUrl
			};
		}

		public static UserDTO ToDto(this Manager manager)
		{
			return new UserDTO
			{
				Id = manager.Id,
				Email = manager.Email,
				FirstName = manager.FirstName,
				LastName = manager.LastName,
				Role = manager.Role.ToString(),
				UserName = manager.UserName,
				ProfilePicUrl = manager.ProfilePictureUrl
			};
		}

		public static UserDTO ToDto(this Admin admin)
		{
			return new UserDTO
			{
				Id = admin.Id,
				Email = admin.Email,
				FirstName = admin.FirstName,
				LastName = admin.LastName,
				Role = admin.Role.ToString(),
				UserName = admin.UserName,
				ProfilePicUrl = string.Empty
			};
		}
	}
}
