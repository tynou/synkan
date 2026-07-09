using Synkan.Application.Interfaces;

namespace Synkan.Application.Services;

public class NotificationService : INotificationService
{
    public async Task SendDeadlineReminder()
    {
        Console.WriteLine("Does this work?");
    }
}