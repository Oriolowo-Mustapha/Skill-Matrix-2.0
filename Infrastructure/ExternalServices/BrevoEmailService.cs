using Application.Interfaces.Service;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices
{
    public class BrevoEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly HttpClient _httpClient;

        public BrevoEmailService(IOptions<EmailSettings> emailSettings, HttpClient httpClient)
        {
            _emailSettings = emailSettings.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new System.Uri("https://api.brevo.com/v3/");
            _httpClient.DefaultRequestHeaders.Add("api-key", _emailSettings.BrevoApiKey);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var payload = new
            {
                sender = new { email = _emailSettings.SenderEmail, name = _emailSettings.SenderName },
                to = new[] { new { email = toEmail } },
                subject = subject,
                textContent = body
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("smtp/email", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Brevo API Error ({response.StatusCode}): {errorJson}");
            }
        }
    }
}
