using Domain.Entities;
using System.Collections.Generic;

namespace Application.DTOs.Ai
{
	public class AssessmentPackageResult
	{
		public List<Assessment> Questions { get; set; } = new List<Assessment>();
		public int TimeLimitMinutes { get; set; } = 30;
	}
}
