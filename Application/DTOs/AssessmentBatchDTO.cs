using System;
using System.Collections.Generic;
using Domain.Enum; // Assuming AssessmentStatus is in Domain.Enum

namespace Application.DTOs
{
    public class AssessmentBatchDTO
    {
        public int Id { get; set; }
        public Guid SkillId { get; set; }
        public Guid? LearnerID { get; set; }
        public Guid? TeamMemberID { get; set; }
        public AssessmentStatus AssessmentStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public List<AssessmentQuestionDTO> Assessments { get; set; } // Assuming AssessmentQuestionDTO exists
    }
}
