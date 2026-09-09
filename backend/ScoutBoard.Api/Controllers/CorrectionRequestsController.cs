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
[Route("api/correction-requests")]
public class CorrectionRequestsController(
    ApplicationDbContext dbContext,
    INotificationService notificationService) : ControllerBase
{
    [Authorize(Roles = UserRoles.Player)]
    [HttpPost("/api/player-statistics/{statisticId:int}/correction-requests")]
    public async Task<ActionResult<CorrectionRequestDto>> Create(
        int statisticId,
        CreateCorrectionRequest request)
    {
        var userId = User.GetUserId();
        var statistic = await dbContext.PlayerMatchStatistics
            .Include(item => item.PlayerProfile)
            .Include(item => item.Match)
                .ThenInclude(match => match.Club)
            .FirstOrDefaultAsync(item => item.Id == statisticId);

        if (statistic is null)
        {
            return NotFound(new MessageResponse("Statistika nije pronađena."));
        }

        if (statistic.PlayerProfile.UserId != userId)
        {
            return Forbid();
        }

        var pendingExists = await dbContext.CorrectionRequests.AnyAsync(item =>
            item.PlayerMatchStatisticId == statisticId &&
            item.Status == CorrectionStatus.Pending);
        if (pendingExists)
        {
            return Conflict(new MessageResponse("Za ovu statistiku već postoji aktivan zahtev."));
        }

        var correction = new CorrectionRequest
        {
            PlayerProfileId = statistic.PlayerProfileId,
            PlayerMatchStatisticId = statisticId,
            Reason = request.Reason.Trim()
        };

        dbContext.CorrectionRequests.Add(correction);
        notificationService.Add(
            statistic.Match.Club.OwnerId,
            "Novi zahtev za ispravku statistike",
            "Igrač je poslao zahtev za ispravku evidentirane statistike.",
            $"/moji-klubovi/{statistic.Match.ClubId}");
        await dbContext.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, Map((await Load(correction.Id))!));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<CorrectionRequestDto>>> GetMine()
    {
        var userId = User.GetUserId();
        var items = await LoadQuery()
            .Where(item => item.PlayerProfile.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
        return Ok(items.Select(Map));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpGet("/api/clubs/{clubId:int}/correction-requests")]
    public async Task<ActionResult<IReadOnlyCollection<CorrectionRequestDto>>> GetForClub(int clubId)
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

        var items = await LoadQuery()
            .Where(item => item.PlayerMatchStatistic.Match.ClubId == clubId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
        return Ok(items.Select(Map));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("{id:int}/resolve")]
    public async Task<ActionResult<CorrectionRequestDto>> Resolve(
        int id,
        ResolveCorrectionRequest request)
    {
        var correction = await dbContext.CorrectionRequests
            .Include(item => item.PlayerProfile)
            .Include(item => item.PlayerMatchStatistic)
                .ThenInclude(statistic => statistic.Match)
                    .ThenInclude(match => match.Club)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (correction is null)
        {
            return NotFound(new MessageResponse("Zahtev nije pronađen."));
        }

        if (correction.PlayerMatchStatistic.Match.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        if (correction.Status != CorrectionStatus.Pending)
        {
            return BadRequest(new MessageResponse("Zahtev je već obrađen."));
        }

        correction.Status = request.Accepted
            ? CorrectionStatus.Accepted
            : CorrectionStatus.Rejected;
        correction.CoachResponse = request.CoachResponse.Trim();
        correction.ResolvedAt = DateTime.UtcNow;
        notificationService.Add(
            correction.PlayerProfile.UserId,
            request.Accepted
                ? "Zahtev za ispravku je prihvaćen"
                : "Zahtev za ispravku je odbijen",
            "Trener je obradio vaš zahtev za ispravku statistike.",
            "/moja-statistika");
        await dbContext.SaveChangesAsync();

        return Ok(Map((await Load(id))!));
    }

    private Task<CorrectionRequest?> Load(int id) =>
        LoadQuery().FirstOrDefaultAsync(item => item.Id == id);

    private IQueryable<CorrectionRequest> LoadQuery() =>
        dbContext.CorrectionRequests
            .AsNoTracking()
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Include(item => item.PlayerMatchStatistic)
                .ThenInclude(statistic => statistic.Match)
                    .ThenInclude(match => match.Club)
            .Include(item => item.PlayerMatchStatistic)
                .ThenInclude(statistic => statistic.Match)
                    .ThenInclude(match => match.OpponentClub);

    private static CorrectionRequestDto Map(CorrectionRequest item)
    {
        var match = item.PlayerMatchStatistic.Match;
        var opponent = match.OpponentClub?.Name ?? match.OpponentName ?? "Nepoznat protivnik";
        return new CorrectionRequestDto(
            item.Id,
            item.PlayerProfileId,
            $"{item.PlayerProfile.User.FirstName} {item.PlayerProfile.User.LastName}",
            item.PlayerMatchStatisticId,
            $"{match.Club.Name} - {opponent}, {match.MatchDate:dd.MM.yyyy.}",
            item.Reason,
            item.Status,
            item.CoachResponse,
            item.CreatedAt,
            item.ResolvedAt);
    }
}
