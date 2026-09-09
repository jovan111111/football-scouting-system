using ScoutBoard.Api.Data;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Services;

public interface INotificationService
{
    void Add(string userId, string title, string message, string? link = null);
}

public class NotificationService(ApplicationDbContext dbContext) : INotificationService
{
    public void Add(string userId, string title, string message, string? link = null)
    {
        dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link
        });
    }
}
