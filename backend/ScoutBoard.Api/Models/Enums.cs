namespace ScoutBoard.Api.Models;

public static class UserRoles
{
    public const string Player = "Player";
    public const string CoachScout = "CoachScout";
    public const string Admin = "Admin";

    public static readonly string[] All = [Player, CoachScout, Admin];
}

public enum City
{
    NoviPazar,
    Tutin,
    Sjenica,
    Raska
}

public enum DominantFoot
{
    Left,
    Right,
    Both
}

public enum FootballPosition
{
    Goalkeeper,
    RightBack,
    CentreBack,
    LeftBack,
    DefensiveMidfielder,
    CentralMidfielder,
    AttackingMidfielder,
    RightWinger,
    LeftWinger,
    Striker
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public enum MembershipStatus
{
    Pending,
    Active,
    Rejected,
    Ended
}

public enum FootballMatchStatus
{
    Scheduled,
    Completed,
    Cancelled
}

public enum ReportVisibility
{
    Public,
    Private
}

public enum RecommendationLevel
{
    NotRecommended,
    Follow,
    Recommended
}

public enum WatchlistStatus
{
    Noticed,
    Contacted,
    Observed,
    Recommended,
    Rejected
}

public enum CorrectionStatus
{
    Pending,
    Accepted,
    Rejected
}

public enum CompetitionStatus
{
    Planned,
    Active,
    Completed
}

public enum TryoutStatus
{
    Open,
    Closed,
    Cancelled
}

public enum TryoutApplicationStatus
{
    Pending,
    Accepted,
    Rejected,
    Cancelled
}
