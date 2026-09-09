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
[Route("api/memberships")]
public class ClubMembershipsController(
    ApplicationDbContext dbContext,
    INotificationService notificationService) : ControllerBase
{
    [Authorize(Roles = UserRoles.CoachScout)]
    [HttpPost("/api/clubs/{clubId:int}/invitations")]
    public async Task<ActionResult<MembershipDto>> SendInvitation(
        int clubId,
        SendInvitationRequest request)
    {
        var userId = User.GetUserId();
        var club = await dbContext.Clubs.FirstOrDefaultAsync(item => item.Id == clubId);

        if (club is null)
        {
            return NotFound(new MessageResponse("Klub nije pronađen."));
        }

        if (club.OwnerId != userId)
        {
            return Forbid();
        }

        if (!club.IsActive || club.ApprovalStatus != ApprovalStatus.Approved)
        {
            return BadRequest(new MessageResponse("Pozivi se mogu slati samo iz odobrenog kluba."));
        }

        var player = await dbContext.PlayerProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile => profile.Id == request.PlayerProfileId);
        if (player is null)
        {
            return NotFound(new MessageResponse("Igrač nije pronađen."));
        }

        var hasActiveMembership = await dbContext.ClubMemberships.AnyAsync(item =>
            item.PlayerProfileId == request.PlayerProfileId &&
            item.Status == MembershipStatus.Active);
        if (hasActiveMembership)
        {
            return Conflict(new MessageResponse("Igrač već ima aktivno članstvo u klubu."));
        }

        var hasPendingInvitation = await dbContext.ClubMemberships.AnyAsync(item =>
            item.ClubId == clubId &&
            item.PlayerProfileId == request.PlayerProfileId &&
            item.Status == MembershipStatus.Pending);
        if (hasPendingInvitation)
        {
            return Conflict(new MessageResponse("Igrač već ima aktivan poziv ovog kluba."));
        }

        var membership = new ClubMembership
        {
            ClubId = clubId,
            PlayerProfileId = request.PlayerProfileId
        };
        dbContext.ClubMemberships.Add(membership);
        notificationService.Add(
            player.UserId,
            "Novi poziv u klub",
            $"{club.Name} vas je pozvao da se pridružite klubu.",
            "/moji-pozivi");
        await dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            PlayersController.MapMembership((await LoadMembership(membership.Id))!));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpGet("my-invitations")]
    public async Task<ActionResult<IReadOnlyCollection<MembershipDto>>> GetMyInvitations()
    {
        var userId = User.GetUserId();
        var memberships = await dbContext.ClubMemberships
            .AsNoTracking()
            .Include(item => item.Club)
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .Where(item =>
                item.PlayerProfile.UserId == userId &&
                item.Status == MembershipStatus.Pending)
            .OrderByDescending(item => item.InvitedAt)
            .ToListAsync();

        return Ok(memberships.Select(PlayersController.MapMembership));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var membership = await dbContext.ClubMemberships
            .Include(item => item.Club)
            .Include(item => item.PlayerProfile)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (membership is null)
        {
            return NotFound(new MessageResponse("Poziv nije pronađen."));
        }

        if (membership.PlayerProfile.UserId != User.GetUserId())
        {
            return Forbid();
        }

        if (membership.Status != MembershipStatus.Pending)
        {
            return BadRequest(new MessageResponse("Poziv više nije aktivan."));
        }

        var hasActiveMembership = await dbContext.ClubMemberships.AnyAsync(item =>
            item.PlayerProfileId == membership.PlayerProfileId &&
            item.Status == MembershipStatus.Active);
        if (hasActiveMembership)
        {
            return Conflict(new MessageResponse("Već imate aktivno članstvo u klubu."));
        }

        membership.Status = MembershipStatus.Active;
        membership.RespondedAt = DateTime.UtcNow;
        membership.JoinedAt = DateTime.UtcNow;
        notificationService.Add(
            membership.Club.OwnerId,
            "Poziv u klub je prihvaćen",
            "Igrač je prihvatio poziv u klub.",
            $"/moji-klubovi/{membership.ClubId}");
        await dbContext.SaveChangesAsync();

        return Ok(new MessageResponse("Poziv je prihvaćen."));
    }

    [Authorize(Roles = UserRoles.Player)]
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var membership = await dbContext.ClubMemberships
            .Include(item => item.Club)
            .Include(item => item.PlayerProfile)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (membership is null)
        {
            return NotFound(new MessageResponse("Poziv nije pronađen."));
        }

        if (membership.PlayerProfile.UserId != User.GetUserId())
        {
            return Forbid();
        }

        if (membership.Status != MembershipStatus.Pending)
        {
            return BadRequest(new MessageResponse("Poziv više nije aktivan."));
        }

        membership.Status = MembershipStatus.Rejected;
        membership.RespondedAt = DateTime.UtcNow;
        notificationService.Add(
            membership.Club.OwnerId,
            "Poziv u klub je odbijen",
            "Igrač je odbio poziv u klub.",
            $"/moji-klubovi/{membership.ClubId}");
        await dbContext.SaveChangesAsync();

        return Ok(new MessageResponse("Poziv je odbijen."));
    }

    [Authorize(Roles = $"{UserRoles.Player},{UserRoles.CoachScout}")]
    [HttpPost("{id:int}/end")]
    public async Task<IActionResult> EndMembership(int id)
    {
        var membership = await dbContext.ClubMemberships
            .Include(item => item.Club)
            .Include(item => item.PlayerProfile)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (membership is null)
        {
            return NotFound(new MessageResponse("Članstvo nije pronađeno."));
        }

        var userId = User.GetUserId();
        if (membership.Club.OwnerId != userId &&
            membership.PlayerProfile.UserId != userId)
        {
            return Forbid();
        }

        if (membership.Status != MembershipStatus.Active)
        {
            return BadRequest(new MessageResponse("Članstvo nije aktivno."));
        }

        membership.Status = MembershipStatus.Ended;
        membership.LeftAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(new MessageResponse("Članstvo je završeno."));
    }

    private Task<ClubMembership?> LoadMembership(int id) =>
        dbContext.ClubMemberships
            .AsNoTracking()
            .Include(item => item.Club)
            .Include(item => item.PlayerProfile)
                .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(item => item.Id == id);
}
