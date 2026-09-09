using System.ComponentModel.DataAnnotations;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.DTOs;

public record MatchListItemDto(
    int Id,
    int ClubId,
    string ClubName,
    int? OpponentClubId,
    string OpponentName,
    int? CompetitionId,
    string? CompetitionName,
    DateTime MatchDate,
    string Venue,
    bool IsHomeMatch,
    int? GoalsScored,
    int? GoalsConceded,
    FootballMatchStatus Status);

public record MatchDetailDto(
    int Id,
    int ClubId,
    string ClubName,
    int? OpponentClubId,
    string OpponentName,
    int? CompetitionId,
    string? CompetitionName,
    DateTime MatchDate,
    string Venue,
    bool IsHomeMatch,
    int? GoalsScored,
    int? GoalsConceded,
    FootballMatchStatus Status,
    string? MatchReport,
    IReadOnlyCollection<MatchPlayerStatisticDto> PlayerStatistics);

public record MatchPlayerStatisticDto(
    int Id,
    int PlayerProfileId,
    string PlayerName,
    bool WasStarter,
    int MinutesPlayed,
    int Goals,
    int Assists,
    int YellowCards,
    bool RedCard,
    decimal? Rating);

public class SaveMatchRequest
{
    public int? CompetitionId { get; set; }

    public int? OpponentClubId { get; set; }

    [MaxLength(120)]
    public string? OpponentName { get; set; }

    public DateTime MatchDate { get; set; }

    [Required, MaxLength(160)]
    public string Venue { get; set; } = string.Empty;

    public bool IsHomeMatch { get; set; }

    [Range(0, 99)]
    public int? GoalsScored { get; set; }

    [Range(0, 99)]
    public int? GoalsConceded { get; set; }

    public FootballMatchStatus Status { get; set; } = FootballMatchStatus.Scheduled;

    [MaxLength(2000)]
    public string? MatchReport { get; set; }
}

public class SaveMatchStatisticsRequest
{
    [Required]
    public List<SavePlayerStatisticRequest> Players { get; set; } = [];
}

public class SavePlayerStatisticRequest
{
    [Range(1, int.MaxValue)]
    public int PlayerProfileId { get; set; }

    public bool WasStarter { get; set; }

    [Range(0, 120)]
    public int MinutesPlayed { get; set; }

    [Range(0, 20)]
    public int Goals { get; set; }

    [Range(0, 20)]
    public int Assists { get; set; }

    [Range(0, 2)]
    public int YellowCards { get; set; }

    public bool RedCard { get; set; }

    [Range(typeof(decimal), "1", "10")]
    public decimal? Rating { get; set; }
}
