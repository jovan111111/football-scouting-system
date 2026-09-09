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
[Route("api/tryouts")]
public class TryoutsController(
    ApplicationDbContext dbContext,
    INotificationService notificationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TryoutListItemDto>>> GetAll(
        City? city,
        FootballPosition? position,
        TryoutStatus? status)
    {
        var query = dbContext.ClubTryouts
            .AsNoTracking()
            .Where(tryout =>
                tryout.Club.IsActive &&
                tryout.Club.ApprovalStatus == ApprovalStatus.Approved);

        if (city.HasValue)
        {
            query = query.Where(tryout => tryout.Club.City == city);
        }

        if (position.HasValue)
        {
            query = query.Where(tryout =>
                tryout.Position == null ||
                tryout.Position == position);
        }

        if (status.HasValue)
        {
            query = query.Where(tryout => tryout.Status == status);
        }

        var tryouts = await query
            .OrderBy(tryout => tryout.TryoutDate)
            .Select(tryout => new TryoutListItemDto(
                tryout.Id,
                tryout.ClubId,
                tryout.Club.Name,
                tryout.Club.City,
                tryout.Title,
                tryout.TryoutDate,
                tryout.Venue,
                tryout.Position,
                tryout.MinimumAge,
                tryout.MaximumAge,
                tryout.Status,
                tryout.Applications.Count(application =>
                    application.Status != TryoutApplicationStatus.Cancelled)))
            .ToListAsync();

        return Ok(tryouts);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TryoutDetailDto>> GetById(int id)
    {
        var tryout = await LoadTryout(id);
        if (tryout is null ||
            !tryout.Club.IsActive ||
            tryout.Club.ApprovalStatus != ApprovalStatus.Approved)
        {
            return NotFound(new MessageResponse("Proba nije pronađena."));
        }

        return Ok(MapDetail(tryout));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPost("/api/clubs/{clubId:int}/tryouts")]
    public async Task<ActionResult<TryoutDetailDto>> Create(
        int clubId,
        SaveTryoutRequest request)
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
            return BadRequest(new MessageResponse(
                "Probu može objaviti samo odobren klub."));
        }

        var validation = ValidateRequest(request);
        if (validation is not null)
        {
            return BadRequest(new MessageResponse(validation));
        }

        var tryout = new ClubTryout { ClubId = clubId };
        ApplyRequest(tryout, request);
        dbContext.ClubTryouts.Add(tryout);
        await dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            MapDetail((await LoadTryout(tryout.Id))!));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TryoutDetailDto>> Update(
        int id,
        SaveTryoutRequest request)
    {
        var tryout = await dbContext.ClubTryouts
            .Include(item => item.Club)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (tryout is null)
        {
            return NotFound(new MessageResponse("Proba nije pronađena."));
        }

        if (tryout.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        var validation = ValidateRequest(request);
        if (validation is not null)
        {
            return BadRequest(new MessageResponse(validation));
        }

        ApplyRequest(tryout, request);
        await dbContext.SaveChangesAsync();
        return Ok(MapDetail((await LoadTryout(id))!));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpPost("{id:int}/apply")]
    public async Task<ActionResult<TryoutApplicationDto>> Apply(
        int id,
        ApplyForTryoutRequest request)
    {
        var userId = User.GetUserId();
        var profile = await dbContext.PlayerProfiles
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.UserId == userId);
        var tryout = await dbContext.ClubTryouts
            .Include(item => item.Club)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (profile is null || tryout is null)
        {
            return NotFound(new MessageResponse("Proba ili profil igrača nisu pronađeni."));
        }

        if (tryout.Status != TryoutStatus.Open ||
            tryout.TryoutDate <= DateTime.UtcNow)
        {
            return BadRequest(new MessageResponse("Prijave za ovu probu nisu otvorene."));
        }

        var existing = await dbContext.TryoutApplications.FirstOrDefaultAsync(item =>
            item.ClubTryoutId == id &&
            item.PlayerProfileId == profile.Id);
        if (existing is not null && existing.Status != TryoutApplicationStatus.Cancelled)
        {
            return Conflict(new MessageResponse("Već ste se prijavili za ovu probu."));
        }

        var ageValidation = ValidatePlayerAge(profile, tryout);
        if (ageValidation is not null)
        {
            return BadRequest(new MessageResponse(ageValidation));
        }

        TryoutApplication application;
        if (existing is null)
        {
            application = new TryoutApplication
            {
                ClubTryoutId = id,
                PlayerProfileId = profile.Id
            };
            dbContext.TryoutApplications.Add(application);
        }
        else
        {
            application = existing;
            application.Status = TryoutApplicationStatus.Pending;
            application.AppliedAt = DateTime.UtcNow;
            application.RespondedAt = null;
            application.CoachNote = null;
        }

        application.Message = request.Message.Trim();
        notificationService.Add(
            tryout.Club.OwnerId,
            "Nova prijava za probu",
            $"{profile.User.FirstName} {profile.User.LastName} se prijavio/la za „{tryout.Title}”.",
            $"/probe/{tryout.Id}");
        await dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            MapApplication((await LoadApplication(application.Id))!));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpPost("applications/{id:int}/cancel")]
    public async Task<IActionResult> CancelApplication(int id)
    {
        var application = await dbContext.TryoutApplications
            .Include(item => item.PlayerProfile)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (application is null)
        {
            return NotFound(new MessageResponse("Prijava nije pronađena."));
        }

        if (application.PlayerProfile.UserId != User.GetUserId())
        {
            return Forbid();
        }

        if (application.Status != TryoutApplicationStatus.Pending)
        {
            return BadRequest(new MessageResponse(
                "Može se otkazati samo prijava koja čeka odgovor."));
        }

        application.Status = TryoutApplicationStatus.Cancelled;
        application.RespondedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return Ok(new MessageResponse("Prijava je otkazana."));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpGet("my-applications")]
    public async Task<ActionResult<IReadOnlyCollection<MyTryoutApplicationDto>>>
        GetMyApplications()
    {
        var userId = User.GetUserId();
        var applications = await dbContext.TryoutApplications
            .AsNoTracking()
            .Where(item => item.PlayerProfile.UserId == userId)
            .OrderByDescending(item => item.AppliedAt)
            .Select(item => new MyTryoutApplicationDto(
                item.Id,
                item.ClubTryoutId,
                item.ClubTryout.Title,
                item.ClubTryout.ClubId,
                item.ClubTryout.Club.Name,
                item.ClubTryout.TryoutDate,
                item.ClubTryout.Venue,
                item.Status,
                item.Message,
                item.CoachNote,
                item.AppliedAt))
            .ToListAsync();

        return Ok(applications);
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpGet("{id:int}/applications")]
    public async Task<ActionResult<IReadOnlyCollection<TryoutApplicationDto>>>
        GetApplications(int id)
    {
        var tryout = await dbContext.ClubTryouts
            .Include(item => item.Club)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (tryout is null)
        {
            return NotFound(new MessageResponse("Proba nije pronađena."));
        }

        if (tryout.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        var applications = await dbContext.TryoutApplications
            .AsNoTracking()
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Where(item => item.ClubTryoutId == id)
            .OrderByDescending(item => item.AppliedAt)
            .ToListAsync();

        return Ok(applications.Select(MapApplication));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("applications/{id:int}/resolve")]
    public async Task<ActionResult<TryoutApplicationDto>> ResolveApplication(
        int id,
        ResolveTryoutApplicationRequest request)
    {
        if (request.Status is not (
            TryoutApplicationStatus.Accepted or TryoutApplicationStatus.Rejected))
        {
            return BadRequest(new MessageResponse(
                "Prijava može biti prihvaćena ili odbijena."));
        }

        var application = await dbContext.TryoutApplications
            .Include(item => item.ClubTryout)
                .ThenInclude(tryout => tryout.Club)
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (application is null)
        {
            return NotFound(new MessageResponse("Prijava nije pronađena."));
        }

        if (application.ClubTryout.Club.OwnerId != User.GetUserId())
        {
            return Forbid();
        }

        if (application.Status != TryoutApplicationStatus.Pending)
        {
            return BadRequest(new MessageResponse("Prijava je već obrađena."));
        }

        application.Status = request.Status;
        application.CoachNote = request.CoachNote?.Trim();
        application.RespondedAt = DateTime.UtcNow;
        var accepted = request.Status == TryoutApplicationStatus.Accepted;
        notificationService.Add(
            application.PlayerProfile.UserId,
            accepted ? "Prijava za probu je prihvaćena" : "Prijava za probu je odbijena",
            $"{application.ClubTryout.Club.Name} je obradio prijavu za „{application.ClubTryout.Title}”.",
            "/moje-prijave-za-probe");
        await dbContext.SaveChangesAsync();

        return Ok(MapApplication(application));
    }

    private Task<ClubTryout?> LoadTryout(int id) =>
        dbContext.ClubTryouts
            .AsNoTracking()
            .Include(tryout => tryout.Club)
            .Include(tryout => tryout.Applications)
                .ThenInclude(application => application.PlayerProfile)
            .FirstOrDefaultAsync(tryout => tryout.Id == id);

    private Task<TryoutApplication?> LoadApplication(int id) =>
        dbContext.TryoutApplications
            .AsNoTracking()
            .Include(application => application.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(application => application.Id == id);

    private TryoutDetailDto MapDetail(ClubTryout tryout)
    {
        var userId = User.GetUserId();
        var myApplication = tryout.Applications
            .FirstOrDefault(application => application.PlayerProfile.UserId == userId);
        return new TryoutDetailDto(
            tryout.Id,
            tryout.ClubId,
            tryout.Club.Name,
            tryout.Club.City,
            tryout.Title,
            tryout.Description,
            tryout.TryoutDate,
            tryout.Venue,
            tryout.Position,
            tryout.MinimumAge,
            tryout.MaximumAge,
            tryout.Status,
            tryout.Applications.Count(application =>
                application.Status != TryoutApplicationStatus.Cancelled),
            tryout.Club.OwnerId == userId,
            myApplication?.Status);
    }

    private static TryoutApplicationDto MapApplication(TryoutApplication application) =>
        new(
            application.Id,
            application.PlayerProfileId,
            $"{application.PlayerProfile.User.FirstName} " +
            application.PlayerProfile.User.LastName,
            application.PlayerProfile.PrimaryPosition,
            application.PlayerProfile.City,
            application.Message,
            application.CoachNote,
            application.Status,
            application.AppliedAt,
            application.RespondedAt);

    private static string? ValidateRequest(SaveTryoutRequest request)
    {
        if (request.Status == TryoutStatus.Open &&
            request.TryoutDate <= DateTime.UtcNow)
        {
            return "Termin otvorene probe mora biti u budućnosti.";
        }

        if (request.MinimumAge.HasValue &&
            request.MaximumAge.HasValue &&
            request.MinimumAge > request.MaximumAge)
        {
            return "Minimalni uzrast ne može biti veći od maksimalnog.";
        }

        return null;
    }

    private static string? ValidatePlayerAge(
        PlayerProfile player,
        ClubTryout tryout)
    {
        if (!tryout.MinimumAge.HasValue && !tryout.MaximumAge.HasValue)
        {
            return null;
        }

        if (!player.DateOfBirth.HasValue)
        {
            return "Unesite datum rođenja na profilu pre prijave.";
        }

        var age = tryout.TryoutDate.Year - player.DateOfBirth.Value.Year;
        if (player.DateOfBirth.Value.Date > tryout.TryoutDate.AddYears(-age).Date)
        {
            age--;
        }

        if (tryout.MinimumAge.HasValue && age < tryout.MinimumAge)
        {
            return "Ne ispunjavate minimalni uzrast za ovu probu.";
        }

        if (tryout.MaximumAge.HasValue && age > tryout.MaximumAge)
        {
            return "Ne ispunjavate maksimalni uzrast za ovu probu.";
        }

        return null;
    }

    private static void ApplyRequest(ClubTryout tryout, SaveTryoutRequest request)
    {
        tryout.Title = request.Title.Trim();
        tryout.Description = request.Description.Trim();
        tryout.TryoutDate = request.TryoutDate;
        tryout.Venue = request.Venue.Trim();
        tryout.Position = request.Position;
        tryout.MinimumAge = request.MinimumAge;
        tryout.MaximumAge = request.MaximumAge;
        tryout.Status = request.Status;
    }
}
