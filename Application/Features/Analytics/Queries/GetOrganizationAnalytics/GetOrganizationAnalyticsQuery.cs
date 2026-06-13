using Application.DTOs;
using Application.DTOs.Analytics;
using MediatR;

namespace Application.Features.Analytics.Queries.GetOrganizationAnalytics
{
	public class GetOrganizationAnalyticsQuery : IRequest<BaseResponse<OrganizationAnalyticsDTO>>
	{
		public Guid OrganizationId { get; set; }
		public Guid RequesterId { get; set; }
		public string RequesterRole { get; set; } = string.Empty;
	}
}
