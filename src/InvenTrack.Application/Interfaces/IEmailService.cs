namespace InvenTrack.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task QueueEmailAsync(string to, string subject, string body);
}
