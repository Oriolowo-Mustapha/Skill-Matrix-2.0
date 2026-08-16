using System;
using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.SaveQuestionResponse
{
	public class SaveQuestionResponseCommand : IRequest<BaseResponse<SaveQuestionResponseResultDTO>>
	{
		public int BatchId { get; set; }
		public int QuestionId { get; set; }
		public SaveQuestionResponseDTO Dto { get; set; }
		public Guid UserId { get; set; }
		public string UserRole { get; set; }

		public SaveQuestionResponseCommand(int batchId, int questionId, SaveQuestionResponseDTO dto, Guid userId, string userRole)
		{
			BatchId = batchId;
			QuestionId = questionId;
			Dto = dto;
			UserId = userId;
			UserRole = userRole;
		}
	}
}
