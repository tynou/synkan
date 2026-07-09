namespace Slate.Application.Interfaces;

public interface INotificationService
{
    Task SendDeadlineReminder();
}