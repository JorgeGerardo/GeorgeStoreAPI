using GeorgeStore.Common;

namespace GeorgeStore.Features.Brevo;

public class BrevoService(HttpClient http) : IEmailSender
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
            throw new Exception($"Brevo error: {error}");
        }
    }
}

