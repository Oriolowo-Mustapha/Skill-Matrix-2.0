using System.Collections.Generic;

namespace Infrastructure.DTOs
{
	public class GeminiAssessmentPackageDto
	{
		public int TimeLimitMinutes { get; set; } = 30;
		public List<GeminiQuestionDto> Questions { get; set; } = new List<GeminiQuestionDto>();
	}
}
