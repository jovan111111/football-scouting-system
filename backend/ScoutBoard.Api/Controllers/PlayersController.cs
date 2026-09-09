using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Route("api/players")]
public class PlayersController(ApplicationDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PlayerListItemDto>>> GetAll(
        string? search,
        City? city,
        FootballPosition? position,
        DominantFoot? dominantFoot,
        bool? lookingForClub)
    {
        var query = dbContext.PlayerProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .Include(profile => profile.Memberships)
                .ThenInclude(membership => membership.Club)
            .Include(profile => profile.MatchStatistics)
                .ThenInclude(statistic => statistic.Match)
            .Where(profile => profile.User.IsActive && profile.User.EmailConfirmed);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(profile =>
                profile.User.FirstName.ToLower().Contains(normalizedSearch) ||
                profile.User.LastName.ToLower().Contains(normalizedSearch));
        }

        if (city.HasValue)
        {
            query = query.Where(profile => profile.City == city);
        }

        if (position.HasValue)
        {
            query = query.Where(profile =>
                profile.PrimaryPosition == position ||
                profile.SecondaryPosition == position);
        }

        if (dominantFoot.HasValue)
        {
            query = query.Where(profile => profile.DominantFoot == dominantFoot);
        }

        if (lookingForClub.HasValue)
        {
            query = query.Where(profile => profile.LookingForClub == lookingForClub);
        }

        var profiles = await query
            .OrderBy(profile => profile.User.FirstName)
            .ThenBy(profile => profile.User.LastName)
            .ToListAsync();

        return Ok(profiles.Select(MapListItem).ToList());
    }

    [AllowAnonymous]
    [HttpGet("compare")]
    public async Task<ActionResult<PlayerComparisonDto>> Compare(
        int firstPlayerId,
        int secondPlayerId,
        string? season)
    {
        if (firstPlayerId == secondPlayerId)
        {
            return BadRequest(new MessageResponse("Izaberite dva različita igrača."));
        }

        var profiles = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .Include(profile => profile.MatchStatistics)
                .ThenInclude(statistic => statistic.Match)
                    .ThenInclude(match => match.Competition)
            .Where(profile =>
                (profile.Id == firstPlayerId || profile.Id == secondPlayerId) &&
                profile.User.IsActive &&
                profile.User.EmailConfirmed)
            .ToListAsync();

        if (profiles.Count != 2)
        {
            return NotFound(new MessageResponse("Jedan ili oba igrača nisu pronađena."));
        }

        var normalizedSeason = string.IsNullOrWhiteSpace(season)
            ? null
            : season.Trim();
        return Ok(new PlayerComparisonDto(
            normalizedSeason,
            MapComparison(
                profiles.First(profile => profile.Id == firstPlayerId),
                normalizedSeason),
            MapComparison(
                profiles.First(profile => profile.Id == secondPlayerId),
                normalizedSeason)));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerDetailDto>> GetById(int id)
    {
        var profile = await LoadProfile(id);
        if (profile is null || !profile.User.IsActive || !profile.User.EmailConfirmed)
        {
            return NotFound(new MessageResponse("Igrač nije pronađen."));
        }

        return Ok(MapDetail(profile));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpGet("me")]
    public async Task<ActionResult<PlayerDetailDto>> GetMine()
    {
        var profile = await LoadCurrentPlayer();
        return profile is null
            ? NotFound(new MessageResponse("Profil igrača nije pronađen."))
            : Ok(MapDetail(profile));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpPut("me")]
    public async Task<ActionResult<PlayerDetailDto>> UpdateMine(UpdatePlayerProfileRequest request)
    {
        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value.Date >= DateTime.UtcNow.Date)
        {
            return BadRequest(new MessageResponse("Datum rođenja mora biti u prošlosti."));
        }

        if (request.PrimaryPosition.HasValue &&
            request.PrimaryPosition == request.SecondaryPosition)
        {
            return BadRequest(new MessageResponse("Primarna i sekundarna pozicija moraju biti različite."));
        }

        var userId = User.GetUserId();
        var profile = await dbContext.PlayerProfiles
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (profile is null)
        {
            return NotFound(new MessageResponse("Profil igrača nije pronađen."));
        }

        profile.DateOfBirth = request.DateOfBirth?.Date;
        profile.HeightCm = request.HeightCm;
        profile.DominantFoot = request.DominantFoot;
        profile.PrimaryPosition = request.PrimaryPosition;
        profile.SecondaryPosition = request.SecondaryPosition;
        profile.City = request.City;
        profile.Biography = request.Biography?.Trim();
        profile.ProfileImageUrl = request.ProfileImageUrl?.Trim();
        profile.LookingForClub = request.LookingForClub;
        profile.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        var updated = await LoadProfile(profile.Id);
        return Ok(MapDetail(updated!));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpGet("me/statistics")]
    public async Task<ActionResult<IReadOnlyCollection<PlayerMatchStatisticDto>>> GetMyStatistics()
    {
        var userId = User.GetUserId();
        var profileId = await dbContext.PlayerProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        if (!profileId.HasValue)
        {
            return NotFound(new MessageResponse("Profil igrača nije pronađen."));
        }

        var statistics = await dbContext.PlayerMatchStatistics
            .AsNoTracking()
            .Include(item => item.Match)
                .ThenInclude(match => match.Club)
            .Include(item => item.Match)
                .ThenInclude(match => match.OpponentClub)
            .Where(item =>
                item.PlayerProfileId == profileId &&
                item.Match.Status == FootballMatchStatus.Completed)
            .OrderByDescending(item => item.Match.MatchDate)
            .ToListAsync();

        return Ok(statistics.Select(item => new PlayerMatchStatisticDto(
            item.Id,
            item.MatchId,
            item.Match.MatchDate,
            item.Match.Club.Name,
            item.Match.OpponentClub?.Name ?? item.Match.OpponentName ?? "Nepoznat protivnik",
            item.Match.GoalsScored,
            item.Match.GoalsConceded,
            item.WasStarter,
            item.MinutesPlayed,
            item.Goals,
            item.Assists,
            item.YellowCards,
            item.RedCard)));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpGet("me/memberships")]
    public async Task<ActionResult<IReadOnlyCollection<MembershipDto>>> GetMyMemberships()
    {
        var userId = User.GetUserId();
        var memberships = await dbContext.ClubMemberships
            .AsNoTracking()
            .Include(item => item.Club)
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Where(item => item.PlayerProfile.UserId == userId)
            .OrderByDescending(item => item.InvitedAt)
            .ToListAsync();

        return Ok(memberships.Select(MapMembership));
    }

    private async Task<PlayerProfile?> LoadCurrentPlayer()
    {
        var userId = User.GetUserId();
        var profileId = await dbContext.PlayerProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        return profileId.HasValue ? await LoadProfile(profileId.Value) : null;
    }

    private Task<PlayerProfile?> LoadProfile(int id) =>
        dbContext.PlayerProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .Include(profile => profile.Memberships)
                .ThenInclude(membership => membership.Club)
            .Include(profile => profile.MatchStatistics)
                .ThenInclude(statistic => statistic.Match)
            .FirstOrDefaultAsync(profile => profile.Id == id);

    private static PlayerComparisonItemDto MapComparison(
        PlayerProfile profile,
        string? season)
    {
        var statistics = profile.MatchStatistics
            .Where(statistic =>
                statistic.Match.Status == FootballMatchStatus.Completed &&
                (season is null || statistic.Match.Competition?.Season == season))
            .ToList();
        var rated = statistics
            .Where(statistic => statistic.Rating.HasValue)
            .Select(statistic => statistic.Rating!.Value)
            .ToList();

        return new PlayerComparisonItemDto(
            profile.Id,
            $"{profile.User.FirstName} {profile.User.LastName}",
            profile.City,
            profile.PrimaryPosition,
            profile.DominantFoot,
            profile.HeightCm,
            profile.ProfileImageUrl,
            new PlayerComparisonStatisticsDto(
                statistics.Count,
                statistics.Sum(statistic => statistic.MinutesPlayed),
                statistics.Sum(statistic => statistic.Goals),
                statistics.Sum(statistic => statistic.Assists),
                statistics.Sum(statistic => statistic.YellowCards),
                statistics.Count(statistic => statistic.RedCard),
                rated.Count == 0 ? null : decimal.Round(rated.Average(), 2)));
    }

    private static PlayerListItemDto MapListItem(PlayerProfile profile)
    {
        var membership = profile.Memberships.FirstOrDefault(item =>
            item.Status == MembershipStatus.Active &&
            item.Club.ApprovalStatus == ApprovalStatus.Approved &&
            item.Club.IsActive);
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
            membership?.ClubId,
            membership?.Club.Name,
            profile.LookingForClub,
            profile.ProfileImageUrl,
            statistics.Count,
            statistics.Sum(item => item.Goals),
            statistics.Sum(item => item.Assists));
    }

    private static PlayerDetailDto MapDetail(PlayerProfile profile)
    {
        var membership = profile.Memberships.FirstOrDefault(item =>
            item.Status == MembershipStatus.Active &&
            item.Club.ApprovalStatus == ApprovalStatus.Approved &&
            item.Club.IsActive);
        var statistics = profile.MatchStatistics
            .Where(item => item.Match.Status == FootballMatchStatus.Completed)
            .ToList();

        var summary = new PlayerStatisticsSummaryDto(
            statistics.Count,
            statistics.Sum(item => item.MinutesPlayed),
            statistics.Sum(item => item.Goals),
            statistics.Sum(item => item.Assists),
            statistics.Sum(item => item.YellowCards),
            statistics.Count(item => item.RedCard));

        return new PlayerDetailDto(
            profile.Id,
            profile.User.FirstName,
            profile.User.LastName,
            profile.DateOfBirth,
            profile.HeightCm,
            profile.DominantFoot,
            profile.PrimaryPosition,
            profile.SecondaryPosition,
            profile.City,
            profile.Biography,
            profile.ProfileImageUrl,
            profile.LookingForClub,
            membership?.ClubId,
            membership?.Club.Name,
            summary);
    }

    internal static MembershipDto MapMembership(ClubMembership item) =>
        new(
            item.Id,
            item.ClubId,
            item.Club.Name,
            item.PlayerProfileId,
            $"{item.PlayerProfile.User.FirstName} {item.PlayerProfile.User.LastName}",
            item.Status,
            item.InvitedAt,
            item.JoinedAt,
            item.LeftAt);
}
