namespace GeorgeStore.Features.Brevo;

public class BrevoOptions
{
    public required string ApiKey { get; set; }
    public required string EmailSender { get; set; }
    public required string ResetPasswordUrl { get; set; }
}
