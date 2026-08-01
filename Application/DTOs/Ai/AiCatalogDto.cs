namespace Application.DTOs.Ai
{
    public class AiCatalogPathDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<AiCatalogTrackDto> Tracks { get; set; } = new();
    }

    public class AiCatalogTrackDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<AiCatalogSkillDto> Skills { get; set; } = new();
    }

    public class AiCatalogSkillDto
    {
        public string SkillName { get; set; } = string.Empty;
        public int TargetLevel { get; set; } = 3;
    }

    public class CatalogGenerationResultDto
    {
        public int CreatedPathsCount { get; set; }
        public int CreatedTracksCount { get; set; }
        public int MappedSkillsCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
