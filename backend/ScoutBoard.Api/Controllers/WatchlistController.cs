using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Authorize(Roles = UserRoles.CoachScout)]
[Route("api/watchlist")]
public class WatchlistController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WatchlistItemDto>>> GetMine()
    {
        var items = await LoadQuery()
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return Ok(items.Select(Map));
    }

    [HttpPost]
    public async Task<ActionResult<WatchlistItemDto>> Add(AddWatchlistItemRequest request)
    {
        var playerExists = await dbContext.PlayerProfiles
            .AnyAsync(profile => profile.Id == request.PlayerProfileId);
        if (!playerExists)
        {
            return NotFound(new MessageResponse("Igrač nije pronađen."));
        }

        var userId = User.GetUserId()!;
        var exists = await dbContext.WatchlistItems.AnyAsync(item =>
            item.CoachScoutId == userId &&
            item.PlayerProfileId == request.PlayerProfileId);
        if (exists)
        {
            return Conflict(new MessageResponse("Igrač je već na listi praćenja."));
        }

        var item = new WatchlistItem
        {
            CoachScoutId = userId,
            PlayerProfileId = request.PlayerProfileId,
            Status = request.Status,
            PrivateNote = request.PrivateNote?.Trim()
        };
        dbContext.WatchlistItems.Add(item);
        await dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            Map((await LoadQuery().FirstAsync(existing => existing.Id == item.Id))));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WatchlistItemDto>> Update(
        int id,
        UpdateWatchlistItemRequest request)
    {
        var item = await dbContext.WatchlistItems.FirstOrDefaultAsync(existing => existing.Id == id);
        if (item is null)
        {
            return NotFound(new MessageResponse("Stavka liste praćenja nije pronađena."));
        }

        if (item.CoachScoutId != User.GetUserId())
        {
            return Forbid();
        }

        item.Status = request.Status;
        item.PrivateNote = request.PrivateNote?.Trim();
        await dbContext.SaveChangesAsync();

        return Ok(Map((await LoadQuery().FirstAsync(existing => existing.Id == id))));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.WatchlistItems.FirstOrDefaultAsync(existing => existing.Id == id);
        if (item is null)
        {
            return NotFound(new MessageResponse("Stavka liste praćenja nije pronađena."));
        }

        if (item.CoachScoutId != User.GetUserId())
        {
            return Forbid();
        }

        dbContext.WatchlistItems.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<WatchlistItem> LoadQuery()
    {
        var userId = User.GetUserId();
        return dbContext.WatchlistItems
            .AsNoTracking()
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.Memberships)
                    .ThenInclude(membership => membership.Club)
            .Where(item => item.CoachScoutId == userId);
    }

    private static WatchlistItemDto Map(WatchlistItem item)
    {
        var membership = item.PlayerProfile.Memberships.FirstOrDefault(existing =>
            existing.Status == MembershipStatus.Active);
        return new WatchlistItemDto(
            item.Id,
            item.PlayerProfileId,
            $"{item.PlayerProfile.User.FirstName} {item.PlayerProfile.User.LastName}",
            item.PlayerProfile.PrimaryPosition,
            item.PlayerProfile.City,
            membership?.Club.Name,
            item.Status,
            item.PrivateNote,
            item.CreatedAt);
    }
}
