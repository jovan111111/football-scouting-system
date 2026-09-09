using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ScoutBoard.Api.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(60)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PlayerProfile? PlayerProfile { get; set; }
    public ICollection<Notification> Notifications { get; set; } = [];
}

public class EmailVerificationCode
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}

public class PlayerProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public int? HeightCm { get; set; }
    public DominantFoot? DominantFoot { get; set; }
    public FootballPosition? PrimaryPosition { get; set; }
    public FootballPosition? SecondaryPosition { get; set; }
    public City? City { get; set; }

    [MaxLength(1000)]
    public string? Biography { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    public bool LookingForClub { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<ClubMembership> Memberships { get; set; } = [];
    public ICollection<PlayerMatchStatistic> MatchStatistics { get; set; } = [];
    public ICollection<ScoutingReport> ScoutingReports { get; set; } = [];
    public ICollection<TryoutApplication> TryoutApplications { get; set; } = [];
}

public class Club
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public City City { get; set; }
    public int? FoundedYear { get; set; }

    [MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? StadiumName { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public string OwnerId { get; set; } = string.Empty;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    [MaxLength(500)]
    public string? AdminNote { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<ClubMembership> Memberships { get; set; } = [];
    public ICollection<FootballMatch> Matches { get; set; } = [];
    public ICollection<CompetitionClub> CompetitionEntries { get; set; } = [];
    public ICollection<ClubTryout> Tryouts { get; set; } = [];
}

public class ClubMembership
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public int PlayerProfileId { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }

    public Club Club { get; set; } = null!;
    public PlayerProfile PlayerProfile { get; set; } = null!;
}

public class FootballMatch
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public int? OpponentClubId { get; set; }
    public int? CompetitionId { get; set; }

    [MaxLength(120)]
    public string? OpponentName { get; set; }

    public DateTime MatchDate { get; set; }

    [MaxLength(160)]
    public string Venue { get; set; } = string.Empty;

    public bool IsHomeMatch { get; set; }
    public int? GoalsScored { get; set; }
    public int? GoalsConceded { get; set; }
    public FootballMatchStatus Status { get; set; } = FootballMatchStatus.Scheduled;

    [MaxLength(2000)]
    public string? MatchReport { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Club Club { get; set; } = null!;
    public Club? OpponentClub { get; set; }
    public Competition? Competition { get; set; }
    public ICollection<PlayerMatchStatistic> PlayerStatistics { get; set; } = [];
}

public class PlayerMatchStatistic
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int PlayerProfileId { get; set; }
    public bool WasStarter { get; set; }
    public int MinutesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public bool RedCard { get; set; }
    public decimal? Rating { get; set; }

    public FootballMatch Match { get; set; } = null!;
    public PlayerProfile PlayerProfile { get; set; } = null!;
    public ICollection<CorrectionRequest> CorrectionRequests { get; set; } = [];
}

public class ScoutingReport
{
    public int Id { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public int PlayerProfileId { get; set; }
    public int Technique { get; set; }
    public int Speed { get; set; }
    public int Passing { get; set; }
    public int Shooting { get; set; }
    public int Defending { get; set; }
    public int PhysicalCondition { get; set; }
    public int GameVision { get; set; }
    public int Teamwork { get; set; }

    [MaxLength(1000)]
    public string Strengths { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Weaknesses { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;

    public FootballPosition? RecommendedPosition { get; set; }
    public RecommendationLevel Recommendation { get; set; }
    public ReportVisibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Author { get; set; } = null!;
    public PlayerProfile PlayerProfile { get; set; } = null!;
}

public class WatchlistItem
{
    public int Id { get; set; }
    public string CoachScoutId { get; set; } = string.Empty;
    public int PlayerProfileId { get; set; }
    public WatchlistStatus Status { get; set; } = WatchlistStatus.Noticed;

    [MaxLength(1000)]
    public string? PrivateNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser CoachScout { get; set; } = null!;
    public PlayerProfile PlayerProfile { get; set; } = null!;
}

public class CorrectionRequest
{
    public int Id { get; set; }
    public int PlayerProfileId { get; set; }
    public int PlayerMatchStatisticId { get; set; }

    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public CorrectionStatus Status { get; set; } = CorrectionStatus.Pending;

    [MaxLength(1000)]
    public string? CoachResponse { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public PlayerProfile PlayerProfile { get; set; } = null!;
    public PlayerMatchStatistic PlayerMatchStatistic { get; set; } = null!;
}

public class Competition
{
    public int Id { get; set; }

    [MaxLength(140)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Season { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionStatus Status { get; set; } = CompetitionStatus.Planned;
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser CreatedBy { get; set; } = null!;
    public ICollection<CompetitionClub> Participants { get; set; } = [];
    public ICollection<FootballMatch> Matches { get; set; } = [];
}

public class CompetitionClub
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public int ClubId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Competition Competition { get; set; } = null!;
    public Club Club { get; set; } = null!;
}

public class ClubTryout
{
    public int Id { get; set; }
    public int ClubId { get; set; }

    [MaxLength(140)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    public DateTime TryoutDate { get; set; }

    [MaxLength(160)]
    public string Venue { get; set; } = string.Empty;

    public FootballPosition? Position { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public TryoutStatus Status { get; set; } = TryoutStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Club Club { get; set; } = null!;
    public ICollection<TryoutApplication> Applications { get; set; } = [];
}

public class TryoutApplication
{
    public int Id { get; set; }
    public int ClubTryoutId { get; set; }
    public int PlayerProfileId { get; set; }

    [MaxLength(700)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(700)]
    public string? CoachNote { get; set; }

    public TryoutApplicationStatus Status { get; set; } =
        TryoutApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    public ClubTryout ClubTryout { get; set; } = null!;
    public PlayerProfile PlayerProfile { get; set; } = null!;
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    [MaxLength(140)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(700)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Link { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
