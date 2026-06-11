namespace Infrastructure.ExternalServices
{
    public class EmailSettings
    {
        public string BrevoApiKey { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
    }
}
