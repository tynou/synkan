namespace Synkan.Application.Interfaces;

public interface INotificationService
{
    Task SendDeadlineReminder();
}