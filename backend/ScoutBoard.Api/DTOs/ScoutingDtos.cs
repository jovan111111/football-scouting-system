using System.ComponentModel.DataAnnotations;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.DTOs;

public record ScoutingReportDto(
    int Id,
    int PlayerProfileId,
    string PlayerName,
    string AuthorId,
    string AuthorName,
    int Technique,
    int Speed,
    int Passing,
    int Shooting,
    int Defending,
    int PhysicalCondition,
    int GameVision,
    int Teamwork,
    string Strengths,
    string Weaknesses,
    string Comment,
    FootballPosition? RecommendedPosition,
    RecommendationLevel Recommendation,
    ReportVisibility Visibility,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class SaveScoutingReportRequest
{
    [Range(1, 10)] public int Technique { get; set; }
    [Range(1, 10)] public int Speed { get; set; }
    [Range(1, 10)] public int Passing { get; set; }
    [Range(1, 10)] public int Shooting { get; set; }
    [Range(1, 10)] public int Defending { get; set; }
    [Range(1, 10)] public int PhysicalCondition { get; set; }
    [Range(1, 10)] public int GameVision { get; set; }
    [Range(1, 10)] public int Teamwork { get; set; }

    [Required, MaxLength(1000)]
    public string Strengths { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Weaknesses { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;

    public FootballPosition? RecommendedPosition { get; set; }
    public RecommendationLevel Recommendation { get; set; }
    public ReportVisibility Visibility { get; set; }
}

public record WatchlistItemDto(
    int Id,
    int PlayerProfileId,
    string PlayerName,
    FootballPosition? Position,
    City? City,
    string? CurrentClubName,
    WatchlistStatus Status,
    string? PrivateNote,
    DateTime CreatedAt);

public class AddWatchlistItemRequest
{
    [Range(1, int.MaxValue)]
    public int PlayerProfileId { get; set; }

    public WatchlistStatus Status { get; set; } = WatchlistStatus.Noticed;

    [MaxLength(1000)]
    public string? PrivateNote { get; set; }
}

public class UpdateWatchlistItemRequest
{
    public WatchlistStatus Status { get; set; }

    [MaxLength(1000)]
    public string? PrivateNote { get; set; }
}
