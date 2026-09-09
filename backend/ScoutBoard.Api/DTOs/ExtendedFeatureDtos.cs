using System.ComponentModel.DataAnnotations;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.DTOs;

public record CompetitionListItemDto(
    int Id,
    string Name,
    string Season,
    DateTime StartDate,
    DateTime EndDate,
    CompetitionStatus Status,
    int ClubCount);

public record CompetitionClubDto(int ClubId, string ClubName, City City);

public record StandingRowDto(
    int Position,
    int ClubId,
    string ClubName,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);

public record CompetitionDetailDto(
    int Id,
    string Name,
    string Season,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    CompetitionStatus Status,
    string CreatedByName,
    bool CanManage,
    IReadOnlyCollection<CompetitionClubDto> Clubs,
    IReadOnlyCollection<StandingRowDto> Standings);

public class SaveCompetitionRequest
{
    [Required, MaxLength(140)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Season { get; set; } = string.Empty;

    [Required, MaxLength(1200)]
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionStatus Status { get; set; } = CompetitionStatus.Planned;
}

public class AddCompetitionClubRequest
{
    [Range(1, int.MaxValue)]
    public int ClubId { get; set; }
}

public record TryoutListItemDto(
    int Id,
    int ClubId,
    string ClubName,
    City City,
    string Title,
    DateTime TryoutDate,
    string Venue,
    FootballPosition? Position,
    int? MinimumAge,
    int? MaximumAge,
    TryoutStatus Status,
    int ApplicationCount);

public record TryoutApplicationDto(
    int Id,
    int PlayerProfileId,
    string PlayerName,
    FootballPosition? Position,
    City? City,
    string Message,
    string? CoachNote,
    TryoutApplicationStatus Status,
    DateTime AppliedAt,
    DateTime? RespondedAt);

public record MyTryoutApplicationDto(
    int Id,
    int TryoutId,
    string TryoutTitle,
    int ClubId,
    string ClubName,
    DateTime TryoutDate,
    string Venue,
    TryoutApplicationStatus Status,
    string Message,
    string? CoachNote,
    DateTime AppliedAt);

public record TryoutDetailDto(
    int Id,
    int ClubId,
    string ClubName,
    City City,
    string Title,
    string Description,
    DateTime TryoutDate,
    string Venue,
    FootballPosition? Position,
    int? MinimumAge,
    int? MaximumAge,
    TryoutStatus Status,
    int ApplicationCount,
    bool CanManage,
    TryoutApplicationStatus? MyApplicationStatus);

public class SaveTryoutRequest
{
    [Required, MaxLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    public DateTime TryoutDate { get; set; }

    [Required, MaxLength(160)]
    public string Venue { get; set; } = string.Empty;

    public FootballPosition? Position { get; set; }

    [Range(10, 60)]
    public int? MinimumAge { get; set; }

    [Range(10, 60)]
    public int? MaximumAge { get; set; }

    public TryoutStatus Status { get; set; } = TryoutStatus.Open;
}

public class ApplyForTryoutRequest
{
    [Required, MaxLength(700)]
    public string Message { get; set; } = string.Empty;
}

public class ResolveTryoutApplicationRequest
{
    [Required]
    public TryoutApplicationStatus Status { get; set; }

    [MaxLength(700)]
    public string? CoachNote { get; set; }
}

public record NotificationDto(
    int Id,
    string Title,
    string Message,
    string? Link,
    bool IsRead,
    DateTime CreatedAt);

public record PlayerComparisonStatisticsDto(
    int Appearances,
    int MinutesPlayed,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards,
    decimal? AverageRating);

public record PlayerComparisonItemDto(
    int PlayerProfileId,
    string FullName,
    City? City,
    FootballPosition? Position,
    DominantFoot? DominantFoot,
    int? HeightCm,
    string? ProfileImageUrl,
    PlayerComparisonStatisticsDto Statistics);

public record PlayerComparisonDto(
    string? Season,
    PlayerComparisonItemDto FirstPlayer,
    PlayerComparisonItemDto SecondPlayer);
