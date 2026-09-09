using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Models;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Authorize(Roles = UserRoles.Admin)]
[Route("api/admin")]
public class AdminController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> Dashboard()
    {
        var result = new AdminDashboardDto(
            await dbContext.Users.CountAsync(),
            await dbContext.PlayerProfiles.CountAsync(),
            await dbContext.Clubs.CountAsync(club => club.IsActive),
            await dbContext.Clubs.CountAsync(club =>
                club.IsActive && club.ApprovalStatus == ApprovalStatus.Pending),
            await dbContext.FootballMatches.CountAsync());
        return Ok(result);
    }

    [HttpGet("clubs")]
    public async Task<ActionResult<IReadOnlyCollection<ClubDetailDto>>> GetClubs(
        ApprovalStatus? status)
    {
        var query = dbContext.Clubs
            .AsNoTracking()
            .Include(club => club.Owner)
            .Include(club => club.Memberships)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(club => club.ApprovalStatus == status);
        }

        var clubs = await query.OrderByDescending(club => club.CreatedAt).ToListAsync();
        return Ok(clubs.Select(club => new ClubDetailDto(
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
            club.Memberships.Count(item => item.Status == MembershipStatus.Active))));
    }

    [HttpPut("clubs/{id:int}/approve")]
    public async Task<IActionResult> ApproveClub(int id)
    {
        var club = await dbContext.Clubs.FirstOrDefaultAsync(item => item.Id == id);
        if (club is null)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        club.ApprovalStatus = ApprovalStatus.Approved;
        club.AdminNote = null;
        club.IsActive = true;
        notificationService.Add(
            club.OwnerId,
            "Klub je odobren",
            $"{club.Name} je odobren i sada je javno vidljiv.",
            $"/moji-klubovi/{club.Id}");
        await dbContext.SaveChangesAsync();
        return Ok(new MessageResponse("Klub je odobren."));
    }

    [HttpPut("clubs/{id:int}/reject")]
    public async Task<IActionResult> RejectClub(int id, RejectClubRequest request)
    {
        var club = await dbContext.Clubs.FirstOrDefaultAsync(item => item.Id == id);
        if (club is null)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        club.ApprovalStatus = ApprovalStatus.Rejected;
        club.AdminNote = request.AdminNote.Trim();
        notificationService.Add(
            club.OwnerId,
            "Klub je odbijen",
            $"{club.Name} nije odobren. Pogledajte napomenu administratora.",
            "/moji-klubovi");
        await dbContext.SaveChangesAsync();
        return Ok(new MessageResponse("Klub je odbijen."));
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<AdminUserDto>>> GetUsers()
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync();

        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
            result.Add(new AdminUserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                role,
                user.IsActive,
                user.EmailConfirmed,
                user.CreatedAt));
        }

        return Ok(result);
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        string id,
        UpdateUserStatusRequest request)
    {
        if (id == User.GetUserId() && !request.IsActive)
        {
            return BadRequest(new MessageResponse("Ne možete deaktivirati sopstveni nalog."));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new MessageResponse("Korisnik nije pronađen."));
        }

        user.IsActive = request.IsActive;
        await userManager.UpdateAsync(user);
        return Ok(new MessageResponse(
            request.IsActive ? "Nalog je aktiviran." : "Nalog je deaktiviran."));
    }
}
