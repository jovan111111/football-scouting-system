using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Route("api/clubs")]
public class ClubsController(ApplicationDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClubListItemDto>>> GetAll(
        string? search,
        City? city)
    {
        var query = dbContext.Clubs
            .AsNoTracking()
            .Include(club => club.Memberships)
            .Where(club =>
                club.IsActive &&
                club.ApprovalStatus == ApprovalStatus.Approved);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(club => club.Name.ToLower().Contains(normalizedSearch));
        }

        if (city.HasValue)
        {
            query = query.Where(club => club.City == city);
        }

        var clubs = await query.OrderBy(club => club.Name).ToListAsync();
        return Ok(clubs.Select(MapListItem));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClubDetailDto>> GetById(int id)
    {
        var club = await LoadClub(id);
        if (club is null ||
            !club.IsActive ||
            club.ApprovalStatus != ApprovalStatus.Approved)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        return Ok(MapDetail(club));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<ClubDetailDto>>> GetMine()
    {
        var userId = User.GetUserId();
        var clubs = await dbContext.Clubs
            .AsNoTracking()
            .Include(club => club.Owner)
            .Include(club => club.Memberships)
            .Where(club => club.OwnerId == userId)
            .OrderByDescending(club => club.CreatedAt)
            .ToListAsync();

        return Ok(clubs.Select(MapDetail));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPost]
    public async Task<ActionResult<ClubDetailDto>> Create(SaveClubRequest request)
    {
        var club = new Club
        {
            Name = request.Name.Trim(),
            City = request.City,
            FoundedYear = request.FoundedYear,
            Description = request.Description.Trim(),
            StadiumName = request.StadiumName?.Trim(),
            Address = request.Address?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            OwnerId = User.GetUserId()!,
            ApprovalStatus = ApprovalStatus.Pending
        };

        dbContext.Clubs.Add(club);
        await dbContext.SaveChangesAsync();

        var created = await LoadClub(club.Id);
        return CreatedAtAction(nameof(GetById), new { id = club.Id }, MapDetail(created!));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClubDetailDto>> Update(int id, SaveClubRequest request)
    {
        var userId = User.GetUserId();
        var club = await dbContext.Clubs.FirstOrDefaultAsync(item => item.Id == id);

        if (club is null)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        if (club.OwnerId != userId)
        {
            return Forbid();
        }

        club.Name = request.Name.Trim();
        club.City = request.City;
        club.FoundedYear = request.FoundedYear;
        club.Description = request.Description.Trim();
        club.StadiumName = request.StadiumName?.Trim();
        club.Address = request.Address?.Trim();
        club.LogoUrl = request.LogoUrl?.Trim();

        if (club.ApprovalStatus == ApprovalStatus.Rejected)
        {
            club.ApprovalStatus = ApprovalStatus.Pending;
            club.AdminNote = null;
        }

        await dbContext.SaveChangesAsync();
        return Ok(MapDetail((await LoadClub(id))!));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/players")]
    public async Task<ActionResult<IReadOnlyCollection<PlayerListItemDto>>> GetPlayers(int id)
    {
        var exists = await dbContext.Clubs.AnyAsync(club =>
            club.Id == id &&
            club.IsActive &&
            club.ApprovalStatus == ApprovalStatus.Approved);

        if (!exists)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        var profiles = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .Include(profile => profile.Memberships)
                .ThenInclude(membership => membership.Club)
            .Include(profile => profile.MatchStatistics)
                .ThenInclude(statistic => statistic.Match)
            .Where(profile => profile.Memberships.Any(membership =>
                membership.ClubId == id &&
                membership.Status == MembershipStatus.Active))
            .OrderBy(profile => profile.User.FirstName)
            .ToListAsync();

        return Ok(profiles.Select(profile =>
        {
            var statistics = profile.MatchStatistics
                .Where(item => item.Match.Status == FootballMatchStatus.Completed)
                .ToList();
            return new PlayerListItemDto(
                profile.Id,
                profile.User.FirstName,
                profile.User.LastName,
                profile.City,
                profile.PrimaryPosition,
                profile.DominantFoot,
                id,
                profile.Memberships.First(item =>
                    item.ClubId == id &&
                    item.Status == MembershipStatus.Active).Club.Name,
                profile.LookingForClub,
                profile.ProfileImageUrl,
                statistics.Count,
                statistics.Sum(item => item.Goals),
                statistics.Sum(item => item.Assists));
        }));
    }

    private Task<Club?> LoadClub(int id) =>
        dbContext.Clubs
            .AsNoTracking()
            .Include(club => club.Owner)
            .Include(club => club.Memberships)
            .FirstOrDefaultAsync(club => club.Id == id);

    private static ClubListItemDto MapListItem(Club club) =>
        new(
            club.Id,
            club.Name,
            club.City,
            club.FoundedYear,
            club.LogoUrl,
            club.StadiumName,
            club.ApprovalStatus,
            club.Memberships.Count(item => item.Status == MembershipStatus.Active));

    private static ClubDetailDto MapDetail(Club club) =>
        new(
            club.Id,
            club.Name,
            club.City,
            club.FoundedYear,
            club.Description,
            club.StadiumName,
            club.Address,
            club.LogoUrl,
            club.ApprovalStatus,
            club.AdminNote,
            $"{club.Owner.FirstName} {club.Owner.LastName}",
            club.Memberships.Count(item => item.Status == MembershipStatus.Active));
}
