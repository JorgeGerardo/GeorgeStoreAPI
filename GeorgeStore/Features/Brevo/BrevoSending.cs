using System.Text.Json.Serialization;

namespace GeorgeStore.Features.Brevo;

public sealed class BrevoEmailRequest
{
    [JsonPropertyName("sender")]
    public BrevoSender Sender { get; set; } = default!;

    [JsonPropertyName("to")]
    public List<BrevoRecipient> To { get; set; } = [];

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = default!;

    [JsonPropertyName("htmlContent")]
    public string HtmlContent { get; set; } = default!;
}

public sealed record BrevoSender(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email
);

public sealed record BrevoRecipient(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string Name
);
