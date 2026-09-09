using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;

namespace ScoutBoard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationDto>>> GetMine(
        bool unreadOnly = false)
    {
        var userId = User.GetUserId();
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(notification => !notification.IsRead);
        }

        var notifications = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .Select(notification => new NotificationDto(
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Link,
                notification.IsRead,
                notification.CreatedAt))
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount()
    {
        var userId = User.GetUserId();
        var count = await dbContext.Notifications.CountAsync(notification =>
            notification.UserId == userId &&
            !notification.IsRead);
        return Ok(new { count });
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(item =>
            item.Id == id &&
            item.UserId == User.GetUserId());
        if (notification is null)
        {
            return NotFound(new MessageResponse("Obaveštenje nije pronađeno."));
        }

        notification.IsRead = true;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.GetUserId();
        var notifications = await dbContext.Notifications
            .Where(notification =>
                notification.UserId == userId &&
                !notification.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
