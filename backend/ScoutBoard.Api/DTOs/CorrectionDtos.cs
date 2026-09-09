using System.ComponentModel.DataAnnotations;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.DTOs;

public record CorrectionRequestDto(
    int Id,
    int PlayerProfileId,
    string PlayerName,
    int PlayerMatchStatisticId,
    string MatchDescription,
    string Reason,
    CorrectionStatus Status,
    string? CoachResponse,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public class CreateCorrectionRequest
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class ResolveCorrectionRequest
{
    public bool Accepted { get; set; }

    [Required, MaxLength(1000)]
    public string CoachResponse { get; set; } = string.Empty;
}

public record AdminDashboardDto(
    int UserCount,
    int PlayerCount,
    int ClubCount,
    int PendingClubCount,
    int MatchCount);

public record AdminUserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt);

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public class RejectClubRequest
{
    [Required, MaxLength(500)]
    public string AdminNote { get; set; } = string.Empty;
}
