namespace Application.DTOs.Assessments
{
    public class TestCaseItem
    {
        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = false;
    }
}
