using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Servicies;

namespace Infrastructure.Implementation.Repositories
{
	public class UserResponseRepository : GenericRepository<UserResponse>, IUserResponseRepository
	{
		public UserResponseRepository(MatrixDbContext context) : base(context)
		{

		}
	}
}
