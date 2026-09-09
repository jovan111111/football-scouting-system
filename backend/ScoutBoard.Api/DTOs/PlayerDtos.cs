using System.ComponentModel.DataAnnotations;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.DTOs;

public record PlayerListItemDto(
    int Id,
    string FirstName,
    string LastName,
    City? City,
    FootballPosition? PrimaryPosition,
    DominantFoot? DominantFoot,
    int? CurrentClubId,
    string? CurrentClubName,
    bool LookingForClub,
    string? ProfileImageUrl,
    int Appearances,
    int Goals,
    int Assists);

public record PlayerDetailDto(
    int Id,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    int? HeightCm,
    DominantFoot? DominantFoot,
    FootballPosition? PrimaryPosition,
    FootballPosition? SecondaryPosition,
    City? City,
    string? Biography,
    string? ProfileImageUrl,
    bool LookingForClub,
    int? CurrentClubId,
    string? CurrentClubName,
    PlayerStatisticsSummaryDto Statistics);

public record PlayerStatisticsSummaryDto(
    int Appearances,
    int MinutesPlayed,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards);

public record PlayerMatchStatisticDto(
    int StatisticId,
    int MatchId,
    DateTime MatchDate,
    string ClubName,
    string OpponentName,
    int? GoalsScored,
    int? GoalsConceded,
    bool WasStarter,
    int MinutesPlayed,
    int Goals,
    int Assists,
    int YellowCards,
    bool RedCard);

public record MembershipDto(
    int Id,
    int ClubId,
    string ClubName,
    int PlayerProfileId,
    string PlayerName,
    MembershipStatus Status,
    DateTime InvitedAt,
    DateTime? JoinedAt,
    DateTime? LeftAt);

public class UpdatePlayerProfileRequest
{
    public DateTime? DateOfBirth { get; set; }

    [Range(140, 220)]
    public int? HeightCm { get; set; }

    public DominantFoot? DominantFoot { get; set; }
    public FootballPosition? PrimaryPosition { get; set; }
    public FootballPosition? SecondaryPosition { get; set; }
    public City? City { get; set; }

    [MaxLength(1000)]
    public string? Biography { get; set; }

    [MaxLength(500), Url]
    public string? ProfileImageUrl { get; set; }

    public bool LookingForClub { get; set; }
}

public class SendInvitationRequest
{
    [Range(1, int.MaxValue)]
    public int PlayerProfileId { get; set; }
}
