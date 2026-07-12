using Microsoft.Extensions.Logging;
using Synkan.Application.Interfaces;

namespace Synkan.Application.Services;

public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendDeadlineReminder()
    {
        logger.LogInformation("Sending a reminder");
    }
}