namespace Application.DTOs.Assessments
{
    public class TestCaseResult
    {
        public int TestCaseIndex { get; set; }
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
        public string ActualOutput { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public bool IsHidden { get; set; }
    }
}
