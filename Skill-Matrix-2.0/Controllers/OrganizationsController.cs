using Application.DTOs;
using Application.Features.Auth.Commands.RegisterOrganization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrganizationsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public OrganizationsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		
	}
}
