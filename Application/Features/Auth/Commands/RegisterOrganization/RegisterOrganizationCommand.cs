using Application.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterOrganization
{
    public record RegisterOrganizationCommand(RegisterOrganizationRequestDTO Request) : IRequest<OrganizationDTO>;
}
