using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Route("api/scouting-reports")]
public class ScoutingReportsController(ApplicationDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("/api/players/{playerId:int}/scouting-reports")]
    public async Task<ActionResult<IReadOnlyCollection<ScoutingReportDto>>> GetForPlayer(int playerId)
    {
        var playerExists = await dbContext.PlayerProfiles.AnyAsync(item => item.Id == playerId);
        if (!playerExists)
        {
            return NotFound(new MessageResponse("Igrač nije pronađen."));
        }

        var userId = User.GetUserId();
        var reports = await dbContext.ScoutingReports
            .AsNoTracking()
            .Include(report => report.Author)
            .Include(report => report.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Where(report =>
                report.PlayerProfileId == playerId &&
                (report.Visibility == ReportVisibility.Public || report.AuthorId == userId))
            .OrderByDescending(report => report.CreatedAt)
            .ToListAsync();

        return Ok(reports.Select(Map));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<ScoutingReportDto>>> GetMine()
    {
        var userId = User.GetUserId();
        var reports = await dbContext.ScoutingReports
            .AsNoTracking()
            .Include(report => report.Author)
            .Include(report => report.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Where(report => report.AuthorId == userId)
            .OrderByDescending(report => report.UpdatedAt)
            .ToListAsync();

        return Ok(reports.Select(Map));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPost("/api/players/{playerId:int}/scouting-reports")]
    public async Task<ActionResult<ScoutingReportDto>> Create(
        int playerId,
        SaveScoutingReportRequest request)
    {
        var playerExists = await dbContext.PlayerProfiles.AnyAsync(item => item.Id == playerId);
        if (!playerExists)
        {
            return NotFound(new MessageResponse("Igrač nije pronađen."));
        }

        var report = new ScoutingReport
        {
            AuthorId = User.GetUserId()!,
            PlayerProfileId = playerId
        };
        ApplyRequest(report, request);

        dbContext.ScoutingReports.Add(report);
        await dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            Map((await Load(report.Id))!));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScoutingReportDto>> Update(
        int id,
        SaveScoutingReportRequest request)
    {
        var report = await dbContext.ScoutingReports.FirstOrDefaultAsync(item => item.Id == id);
        if (report is null)
        {
            return NotFound(new MessageResponse("Skautski izveštaj nije pronađen."));
        }

        if (report.AuthorId != User.GetUserId())
        {
            return Forbid();
        }

        ApplyRequest(report, request);
        report.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(Map((await Load(id))!));
    }

    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await dbContext.ScoutingReports.FirstOrDefaultAsync(item => item.Id == id);
        if (report is null)
        {
            return NotFound(new MessageResponse("Skautski izveštaj nije pronađen."));
        }

        if (report.AuthorId != User.GetUserId())
        {
            return Forbid();
        }

        dbContext.ScoutingReports.Remove(report);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private Task<ScoutingReport?> Load(int id) =>
        dbContext.ScoutingReports
            .AsNoTracking()
            .Include(report => report.Author)
            .Include(report => report.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(report => report.Id == id);

    private static void ApplyRequest(
        ScoutingReport report,
        SaveScoutingReportRequest request)
    {
        report.Technique = request.Technique;
        report.Speed = request.Speed;
        report.Passing = request.Passing;
        report.Shooting = request.Shooting;
        report.Defending = request.Defending;
        report.PhysicalCondition = request.PhysicalCondition;
        report.GameVision = request.GameVision;
        report.Teamwork = request.Teamwork;
        report.Strengths = request.Strengths.Trim();
        report.Weaknesses = request.Weaknesses.Trim();
        report.Comment = request.Comment.Trim();
        report.RecommendedPosition = request.RecommendedPosition;
        report.Recommendation = request.Recommendation;
        report.Visibility = request.Visibility;
    }

    private static ScoutingReportDto Map(ScoutingReport report) =>
        new(
            report.Id,
            report.PlayerProfileId,
            $"{report.PlayerProfile.User.FirstName} {report.PlayerProfile.User.LastName}",
            report.AuthorId,
            $"{report.Author.FirstName} {report.Author.LastName}",
            report.Technique,
            report.Speed,
            report.Passing,
            report.Shooting,
            report.Defending,
            report.PhysicalCondition,
            report.GameVision,
            report.Teamwork,
            report.Strengths,
            report.Weaknesses,
            report.Comment,
            report.RecommendedPosition,
            report.Recommendation,
            report.Visibility,
            report.CreatedAt,
            report.UpdatedAt);
}
