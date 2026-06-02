using GeorgeStore.Common.Core.Interfaces;

namespace GeorgeStore.Infrastructure.Email.Brevo;

public class BrevoService(HttpClient http, ILogger<BrevoService> logger) : IEmailSender
{
    public async Task Send(string Subject, string sender, string SenderName, string Target, string TargetName, string HtmlContent)
    {
        BrevoEmailRequest body = new()
        {
            Sender = new BrevoSender(SenderName, sender),
            To = new List<BrevoRecipient>
            {
                new BrevoRecipient(Target, TargetName)
            },
            Subject = Subject,
            HtmlContent = HtmlContent
        };
        HttpResponseMessage response = await http.PostAsJsonAsync("smtp/email", body);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError(error ?? "No message available");
            logger.LogError(
                "Error calling Brevo. StatusCode: {StatusCode}. Response: {Response}",
                response.StatusCode,
                error
            );
            throw new Exception($"Brevo error: {error}");
        }
    }
}

