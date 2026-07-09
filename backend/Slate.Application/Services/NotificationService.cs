using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class NotificationService : INotificationService
{
    public async Task SendDeadlineReminder()
    {
        Console.WriteLine("Does this work?");
    }
}