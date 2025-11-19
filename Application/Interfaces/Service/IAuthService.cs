using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface IAuthService
	{
		Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
		Task<UserDTO> RegisterLearnerAsync(RegisterLearnerRequestDTO request);
		Task<UserDTO> RegisterTeamMemberAsync(RegisterTeamMemberRequestDTO request, Guid ManagerId);
		Task<UserDTO> RegisterManagerAsync(RegisterManagerRequestDTO request);
		Task<OrganizationDTO> RegisterOrganizationAsync(RegisterOrganizationRequestDTO request);
	}
}
