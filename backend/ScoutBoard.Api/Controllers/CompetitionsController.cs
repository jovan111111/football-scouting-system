using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Route("api/competitions")]
public class CompetitionsController(ApplicationDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompetitionListItemDto>>> GetAll()
    {
        var competitions = await dbContext.Competitions
            .AsNoTracking()
            .OrderByDescending(competition => competition.StartDate)
            .Select(competition => new CompetitionListItemDto(
                competition.Id,
                competition.Name,
                competition.Season,
                competition.StartDate,
                competition.EndDate,
                competition.Status,
                competition.Participants.Count))
            .ToListAsync();

        return Ok(competitions);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompetitionDetailDto>> GetById(int id)
    {
        var competition = await LoadCompetition(id);
        if (competition is null)
        {
            return NotFound(new MessageResponse("Takmičenje nije pronađeno."));
        }

        return Ok(MapDetail(competition));
    }

    [Authorize(Roles = $"{UserRoles.CoachScout},{UserRoles.Admin}")]
    [HttpPost]
    public async Task<ActionResult<CompetitionDetailDto>> Create(
        SaveCompetitionRequest request)
    {
        var validation = ValidateRequest(request);
        if (validation is not null)
        {
            return BadRequest(new MessageResponse(validation));
        }

        var competition = new Competition
        {
            CreatedById = User.GetUserId()!
        };
        ApplyRequest(competition, request);
        dbContext.Competitions.Add(competition);
        await dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            MapDetail((await LoadCompetition(competition.Id))!));
    }

    [Authorize(Roles = $"{UserRoles.CoachScout},{UserRoles.Admin}")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CompetitionDetailDto>> Update(
        int id,
        SaveCompetitionRequest request)
    {
        var competition = await dbContext.Competitions
            .FirstOrDefaultAsync(item => item.Id == id);
        if (competition is null)
        {
            return NotFound(new MessageResponse("Takmičenje nije pronađeno."));
        }

        if (!CanManage(competition))
        {
            return Forbid();
        }

        var validation = ValidateRequest(request);
        if (validation is not null)
        {
            return BadRequest(new MessageResponse(validation));
        }

        ApplyRequest(competition, request);
        await dbContext.SaveChangesAsync();
        return Ok(MapDetail((await LoadCompetition(id))!));
    }

    [Authorize(Roles = $"{UserRoles.CoachScout},{UserRoles.Admin}")]
    [HttpPost("{id:int}/clubs")]
    public async Task<ActionResult<CompetitionDetailDto>> AddClub(
        int id,
        AddCompetitionClubRequest request)
    {
        var competition = await dbContext.Competitions
            .Include(item => item.Participants)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (competition is null)
        {
            return NotFound(new MessageResponse("Takmičenje nije pronađeno."));
        }

        if (!CanManage(competition))
        {
            return Forbid();
        }

        var clubExists = await dbContext.Clubs.AnyAsync(club =>
            club.Id == request.ClubId &&
            club.IsActive &&
            club.ApprovalStatus == ApprovalStatus.Approved);
        if (!clubExists)
        {
            return NotFound(new MessageResponse("Odobren klub nije pronađen."));
        }

        if (competition.Participants.Any(entry => entry.ClubId == request.ClubId))
        {
            return Conflict(new MessageResponse("Klub je već dodat u takmičenje."));
        }

        competition.Participants.Add(new CompetitionClub { ClubId = request.ClubId });
        await dbContext.SaveChangesAsync();
        return Ok(MapDetail((await LoadCompetition(id))!));
    }

    [Authorize(Roles = $"{UserRoles.CoachScout},{UserRoles.Admin}")]
    [HttpDelete("{id:int}/clubs/{clubId:int}")]
    public async Task<IActionResult> RemoveClub(int id, int clubId)
    {
        var competition = await dbContext.Competitions
            .Include(item => item.Participants)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (competition is null)
        {
            return NotFound(new MessageResponse("Takmičenje nije pronađeno."));
        }

        if (!CanManage(competition))
        {
            return Forbid();
        }

        var entry = competition.Participants.FirstOrDefault(item => item.ClubId == clubId);
        if (entry is null)
        {
            return NotFound(new MessageResponse("Klub nije učesnik ovog takmičenja."));
        }

        var hasMatches = await dbContext.FootballMatches.AnyAsync(match =>
            match.CompetitionId == id &&
            (match.ClubId == clubId || match.OpponentClubId == clubId));
        if (hasMatches)
        {
            return BadRequest(new MessageResponse(
                "Klub sa evidentiranom utakmicom ne može biti uklonjen."));
        }

        dbContext.CompetitionClubs.Remove(entry);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private bool CanManage(Competition competition) =>
        User.IsInRole(UserRoles.Admin) ||
        competition.CreatedById == User.GetUserId();

    private Task<Competition?> LoadCompetition(int id) =>
        dbContext.Competitions
            .AsNoTracking()
            .Include(competition => competition.CreatedBy)
            .Include(competition => competition.Participants)
                .ThenInclude(entry => entry.Club)
            .Include(competition => competition.Matches)
            .FirstOrDefaultAsync(competition => competition.Id == id);

    private CompetitionDetailDto MapDetail(Competition competition)
    {
        var rows = competition.Participants.ToDictionary(
            entry => entry.ClubId,
            entry => new StandingAccumulator(entry.ClubId, entry.Club.Name));

        foreach (var match in competition.Matches.Where(match =>
                     match.Status == FootballMatchStatus.Completed &&
                     match.OpponentClubId.HasValue &&
                     match.GoalsScored.HasValue &&
                     match.GoalsConceded.HasValue))
        {
            if (!rows.TryGetValue(match.ClubId, out var club) ||
                !rows.TryGetValue(match.OpponentClubId!.Value, out var opponent))
            {
                continue;
            }

            club.Add(match.GoalsScored!.Value, match.GoalsConceded!.Value);
            opponent.Add(match.GoalsConceded.Value, match.GoalsScored.Value);
        }

        var standings = rows.Values
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenBy(row => row.ClubName)
            .Select((row, index) => row.ToDto(index + 1))
            .ToList();

        return new CompetitionDetailDto(
            competition.Id,
            competition.Name,
            competition.Season,
            competition.Description,
            competition.StartDate,
            competition.EndDate,
            competition.Status,
            $"{competition.CreatedBy.FirstName} {competition.CreatedBy.LastName}",
            CanManage(competition),
            competition.Participants
                .OrderBy(entry => entry.Club.Name)
                .Select(entry => new CompetitionClubDto(
                    entry.ClubId,
                    entry.Club.Name,
                    entry.Club.City))
                .ToList(),
            standings);
    }

    private static string? ValidateRequest(SaveCompetitionRequest request)
    {
        if (request.StartDate.Date > request.EndDate.Date)
        {
            return "Datum završetka mora biti posle datuma početka.";
        }

        return null;
    }

    private static void ApplyRequest(
        Competition competition,
        SaveCompetitionRequest request)
    {
        competition.Name = request.Name.Trim();
        competition.Season = request.Season.Trim();
        competition.Description = request.Description.Trim();
        competition.StartDate = request.StartDate.Date;
        competition.EndDate = request.EndDate.Date;
        competition.Status = request.Status;
    }

    private sealed class StandingAccumulator(int clubId, string clubName)
    {
        public int ClubId { get; } = clubId;
        public string ClubName { get; } = clubName;
        public int Played { get; private set; }
        public int Won { get; private set; }
        public int Drawn { get; private set; }
        public int Lost { get; private set; }
        public int GoalsFor { get; private set; }
        public int GoalsAgainst { get; private set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points => Won * 3 + Drawn;

        public void Add(int goalsFor, int goalsAgainst)
        {
            Played++;
            GoalsFor += goalsFor;
            GoalsAgainst += goalsAgainst;
            if (goalsFor > goalsAgainst)
            {
                Won++;
            }
            else if (goalsFor == goalsAgainst)
            {
                Drawn++;
            }
            else
            {
                Lost++;
            }
        }

        public StandingRowDto ToDto(int position) =>
            new(
                position,
                ClubId,
                ClubName,
                Played,
                Won,
                Drawn,
                Lost,
                GoalsFor,
                GoalsAgainst,
                GoalDifference,
                Points);
    }
}
