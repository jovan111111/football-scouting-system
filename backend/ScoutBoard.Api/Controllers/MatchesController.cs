using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController(
    ApplicationDbContext dbContext,
    INotificationService notificationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MatchListItemDto>>> GetAll(
        int? clubId,
        int? competitionId,
        City? city,
        DateTime? from,
        DateTime? to)
    {
        var query = dbContext.FootballMatches
            .AsNoTracking()
            .Include(match => match.Club)
            .Include(match => match.OpponentClub)
            .Include(match => match.Competition)
            .Where(match =>
                match.Club.IsActive &&
                match.Club.ApprovalStatus == ApprovalStatus.Approved);

        if (clubId.HasValue)
        {
            query = query.Where(match =>
                match.ClubId == clubId ||
                match.OpponentClubId == clubId);
        }

        if (competitionId.HasValue)
        {
            query = query.Where(match => match.CompetitionId == competitionId);
        }

        if (city.HasValue)
        {
            query = query.Where(match => match.Club.City == city);
        }

        if (from.HasValue)
        {
            query = query.Where(match => match.MatchDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(match => match.MatchDate <= to.Value);
        }

        var matches = await query
            .OrderByDescending(match => match.MatchDate)
            .ToListAsync();

        return Ok(matches.Select(MapListItem));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatchDetailDto>> GetById(int id)
    {
        var match = await LoadMatch(id);
        if (match is null ||
            !match.Club.IsActive ||
            match.Club.ApprovalStatus != ApprovalStatus.Approved)
        {
            return NotFound(new MessageResponse("Utakmica nije pronađena."));
        }

        return Ok(MapDetail(match));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPost("/api/clubs/{clubId:int}/matches")]
    public async Task<ActionResult<MatchDetailDto>> Create(
        int clubId,
        SaveMatchRequest request)
    {
        var club = await dbContext.Clubs.FirstOrDefaultAsync(item => item.Id == clubId);
        if (club is null)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        if (club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        if (!club.IsActive || club.ApprovalStatus != ApprovalStatus.Approved)
        {
            return BadRequest(new MessageResponse("Utakmica se može dodati samo odobrenom klubu."));
        }

        var validationMessage = await ValidateRequest(clubId, request);
        if (validationMessage is not null)
        {
            return BadRequest(new MessageResponse(validationMessage));
        }

        var match = new FootballMatch { ClubId = clubId };
        ApplyRequest(match, request);

        dbContext.FootballMatches.Add(match);
        await dbContext.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, MapDetail((await LoadMatch(match.Id))!));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MatchDetailDto>> Update(int id, SaveMatchRequest request)
    {
        var match = await dbContext.FootballMatches
            .Include(item => item.Club)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (match is null)
        {
            return NotFound(new MessageResponse("Utakmica nije pronađena."));
        }

        if (match.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        var validationMessage = await ValidateRequest(match.ClubId, request);
        if (validationMessage is not null)
        {
            return BadRequest(new MessageResponse(validationMessage));
        }

        ApplyRequest(match, request);
        await dbContext.SaveChangesAsync();

        return Ok(MapDetail((await LoadMatch(id))!));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var match = await dbContext.FootballMatches
            .Include(item => item.Club)
            .Include(item => item.PlayerStatistics)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (match is null)
        {
            return NotFound(new MessageResponse("Utakmica nije pronađena."));
        }

        if (match.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        if (match.Status != FootballMatchStatus.Scheduled || match.PlayerStatistics.Count != 0)
        {
            return BadRequest(new MessageResponse(
                "Može se obrisati samo zakazana utakmica bez statistike."));
        }

        dbContext.FootballMatches.Remove(match);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("{id:int}/statistics")]
    public async Task<ActionResult<MatchDetailDto>> SaveStatistics(
        int id,
        SaveMatchStatisticsRequest request)
    {
        if (request.Players.Select(item => item.PlayerProfileId).Distinct().Count() !=
            request.Players.Count)
        {
            return BadRequest(new MessageResponse("Isti igrač je dodat više puta."));
        }

        var match = await dbContext.FootballMatches
            .Include(item => item.Club)
            .Include(item => item.PlayerStatistics)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (match is null)
        {
            return NotFound(new MessageResponse("Utakmica nije pronađena."));
        }

        if (match.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        if (match.Status != FootballMatchStatus.Completed)
        {
            return BadRequest(new MessageResponse(
                "Statistika se unosi tek kada je utakmica završena."));
        }

        var playerIds = request.Players.Select(item => item.PlayerProfileId).ToList();
        var validPlayerIds = await dbContext.ClubMemberships
            .Where(item =>
                item.ClubId == match.ClubId &&
                item.Status == MembershipStatus.Active &&
                playerIds.Contains(item.PlayerProfileId))
            .Select(item => item.PlayerProfileId)
            .ToListAsync();

        if (validPlayerIds.Count != playerIds.Count)
        {
            return BadRequest(new MessageResponse(
                "Statistika se može uneti samo aktivnim igračima kluba."));
        }

        var removed = match.PlayerStatistics
            .Where(existing => !playerIds.Contains(existing.PlayerProfileId))
            .ToList();
        dbContext.PlayerMatchStatistics.RemoveRange(removed);

        foreach (var player in request.Players)
        {
            var statistic = match.PlayerStatistics
                .FirstOrDefault(item => item.PlayerProfileId == player.PlayerProfileId);

            if (statistic is null)
            {
                statistic = new PlayerMatchStatistic
                {
                    MatchId = match.Id,
                    PlayerProfileId = player.PlayerProfileId
                };
                dbContext.PlayerMatchStatistics.Add(statistic);
            }

            statistic.WasStarter = player.WasStarter;
            statistic.MinutesPlayed = player.MinutesPlayed;
            statistic.Goals = player.Goals;
            statistic.Assists = player.Assists;
            statistic.YellowCards = player.YellowCards;
            statistic.RedCard = player.RedCard;
            statistic.Rating = player.Rating;
        }

        var playersToNotify = await dbContext.PlayerProfiles
            .Where(profile => playerIds.Contains(profile.Id))
            .Select(profile => new { profile.Id, profile.UserId })
            .ToListAsync();
        foreach (var player in playersToNotify)
        {
            notificationService.Add(
                player.UserId,
                "Evidentirana statistika",
                $"Uneta je vaša statistika za utakmicu kluba {match.Club.Name}.",
                "/moja-statistika");
        }

        await dbContext.SaveChangesAsync();
        return Ok(MapDetail((await LoadMatch(id))!));
    }

    private async Task<string?> ValidateRequest(int clubId, SaveMatchRequest request)
    {
        var hasRegisteredOpponent = request.OpponentClubId.HasValue;
        var hasManualOpponent = !string.IsNullOrWhiteSpace(request.OpponentName);

        if (hasRegisteredOpponent == hasManualOpponent)
        {
            return "Izaberite registrovani klub ili unesite naziv protivnika.";
        }

        if (request.OpponentClubId == clubId)
        {
            return "Klub ne može igrati protiv samog sebe.";
        }

        if (request.OpponentClubId.HasValue)
        {
            var opponentExists = await dbContext.Clubs.AnyAsync(club =>
                club.Id == request.OpponentClubId &&
                club.IsActive &&
                club.ApprovalStatus == ApprovalStatus.Approved);
            if (!opponentExists)
            {
                return "Izabrani protivnički klub nije pronađen.";
            }
        }

        if (request.CompetitionId.HasValue)
        {
            if (!request.OpponentClubId.HasValue)
            {
                return "Utakmica u takmičenju mora imati registrovanog protivnika.";
            }

            var participantIds = await dbContext.CompetitionClubs
                .Where(entry =>
                    entry.CompetitionId == request.CompetitionId &&
                    (entry.ClubId == clubId ||
                     entry.ClubId == request.OpponentClubId.Value))
                .Select(entry => entry.ClubId)
                .Distinct()
                .ToListAsync();
            if (participantIds.Count != 2)
            {
                return "Oba kluba moraju biti učesnici izabranog takmičenja.";
            }
        }

        if (request.Status == FootballMatchStatus.Completed &&
            (!request.GoalsScored.HasValue || !request.GoalsConceded.HasValue))
        {
            return "Za završenu utakmicu potrebno je uneti rezultat.";
        }

        return null;
    }

    private static void ApplyRequest(FootballMatch match, SaveMatchRequest request)
    {
        match.OpponentClubId = request.OpponentClubId;
        match.CompetitionId = request.CompetitionId;
        match.OpponentName = request.OpponentClubId.HasValue
            ? null
            : request.OpponentName?.Trim();
        match.MatchDate = request.MatchDate;
        match.Venue = request.Venue.Trim();
        match.IsHomeMatch = request.IsHomeMatch;
        match.GoalsScored = request.GoalsScored;
        match.GoalsConceded = request.GoalsConceded;
        match.Status = request.Status;
        match.MatchReport = request.MatchReport?.Trim();
    }

    private Task<FootballMatch?> LoadMatch(int id) =>
        dbContext.FootballMatches
            .AsNoTracking()
            .Include(match => match.Club)
            .Include(match => match.OpponentClub)
            .Include(match => match.Competition)
            .Include(match => match.PlayerStatistics)
                .ThenInclude(statistic => statistic.PlayerProfile)
                    .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(match => match.Id == id);

    private static MatchListItemDto MapListItem(FootballMatch match) =>
        new(
            match.Id,
            match.ClubId,
            match.Club.Name,
            match.OpponentClubId,
            match.OpponentClub?.Name ?? match.OpponentName ?? "Nepoznat protivnik",
            match.CompetitionId,
            match.Competition?.Name,
            match.MatchDate,
            match.Venue,
            match.IsHomeMatch,
            match.GoalsScored,
            match.GoalsConceded,
            match.Status);

    private static MatchDetailDto MapDetail(FootballMatch match) =>
        new(
            match.Id,
            match.ClubId,
            match.Club.Name,
            match.OpponentClubId,
            match.OpponentClub?.Name ?? match.OpponentName ?? "Nepoznat protivnik",
            match.CompetitionId,
            match.Competition?.Name,
            match.MatchDate,
            match.Venue,
            match.IsHomeMatch,
            match.GoalsScored,
            match.GoalsConceded,
            match.Status,
            match.MatchReport,
            match.PlayerStatistics
                .OrderByDescending(item => item.WasStarter)
                .ThenBy(item => item.PlayerProfile.User.FirstName)
                .Select(item => new MatchPlayerStatisticDto(
                    item.Id,
                    item.PlayerProfileId,
                    $"{item.PlayerProfile.User.FirstName} {item.PlayerProfile.User.LastName}",
                    item.WasStarter,
                    item.MinutesPlayed,
                    item.Goals,
                    item.Assists,
                    item.YellowCards,
                    item.RedCard,
                    item.Rating))
                .ToList());
}
