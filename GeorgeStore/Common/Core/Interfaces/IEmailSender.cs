namespace GeorgeStore.Common.Core.Interfaces;

public interface IEmailSender
{
    Task Send(string Subject, string sender, string SenderName, string Target, string TargetName, string HtmlContent);
}
