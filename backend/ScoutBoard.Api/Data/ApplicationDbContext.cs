using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<ClubMembership> ClubMemberships => Set<ClubMembership>();
    public DbSet<FootballMatch> FootballMatches => Set<FootballMatch>();
    public DbSet<PlayerMatchStatistic> PlayerMatchStatistics => Set<PlayerMatchStatistic>();
    public DbSet<ScoutingReport> ScoutingReports => Set<ScoutingReport>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<CorrectionRequest> CorrectionRequests => Set<CorrectionRequest>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<CompetitionClub> CompetitionClubs => Set<CompetitionClub>();
    public DbSet<ClubTryout> ClubTryouts => Set<ClubTryout>();
    public DbSet<TryoutApplication> TryoutApplications => Set<TryoutApplication>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(user => user.PlayerProfile)
            .WithOne(profile => profile.User)
            .HasForeignKey<PlayerProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlayerProfile>()
            .HasIndex(profile => profile.UserId)
            .IsUnique();

        builder.Entity<Club>()
            .HasOne(club => club.Owner)
            .WithMany()
            .HasForeignKey(club => club.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ClubMembership>()
            .HasOne(membership => membership.Club)
            .WithMany(club => club.Memberships)
            .HasForeignKey(membership => membership.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ClubMembership>()
            .HasOne(membership => membership.PlayerProfile)
            .WithMany(profile => profile.Memberships)
            .HasForeignKey(membership => membership.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FootballMatch>()
            .HasOne(match => match.Club)
            .WithMany(club => club.Matches)
            .HasForeignKey(match => match.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FootballMatch>()
            .HasOne(match => match.OpponentClub)
            .WithMany()
            .HasForeignKey(match => match.OpponentClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FootballMatch>()
            .HasOne(match => match.Competition)
            .WithMany(competition => competition.Matches)
            .HasForeignKey(match => match.CompetitionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PlayerMatchStatistic>()
            .HasIndex(statistic => new { statistic.MatchId, statistic.PlayerProfileId })
            .IsUnique();

        builder.Entity<PlayerMatchStatistic>()
            .HasOne(statistic => statistic.Match)
            .WithMany(match => match.PlayerStatistics)
            .HasForeignKey(statistic => statistic.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlayerMatchStatistic>()
            .HasOne(statistic => statistic.PlayerProfile)
            .WithMany(profile => profile.MatchStatistics)
            .HasForeignKey(statistic => statistic.PlayerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ScoutingReport>()
            .HasOne(report => report.Author)
            .WithMany()
            .HasForeignKey(report => report.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ScoutingReport>()
            .HasOne(report => report.PlayerProfile)
            .WithMany(profile => profile.ScoutingReports)
            .HasForeignKey(report => report.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WatchlistItem>()
            .HasIndex(item => new { item.CoachScoutId, item.PlayerProfileId })
            .IsUnique();

        builder.Entity<WatchlistItem>()
            .HasOne(item => item.CoachScout)
            .WithMany()
            .HasForeignKey(item => item.CoachScoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WatchlistItem>()
            .HasOne(item => item.PlayerProfile)
            .WithMany()
            .HasForeignKey(item => item.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CorrectionRequest>()
            .HasOne(request => request.PlayerProfile)
            .WithMany()
            .HasForeignKey(request => request.PlayerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CorrectionRequest>()
            .HasOne(request => request.PlayerMatchStatistic)
            .WithMany(statistic => statistic.CorrectionRequests)
            .HasForeignKey(request => request.PlayerMatchStatisticId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmailVerificationCode>()
            .HasOne(code => code.User)
            .WithMany()
            .HasForeignKey(code => code.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Competition>()
            .HasOne(competition => competition.CreatedBy)
            .WithMany()
            .HasForeignKey(competition => competition.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CompetitionClub>()
            .HasIndex(entry => new { entry.CompetitionId, entry.ClubId })
            .IsUnique();

        builder.Entity<CompetitionClub>()
            .HasOne(entry => entry.Competition)
            .WithMany(competition => competition.Participants)
            .HasForeignKey(entry => entry.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CompetitionClub>()
            .HasOne(entry => entry.Club)
            .WithMany(club => club.CompetitionEntries)
            .HasForeignKey(entry => entry.ClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ClubTryout>()
            .HasOne(tryout => tryout.Club)
            .WithMany(club => club.Tryouts)
            .HasForeignKey(tryout => tryout.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TryoutApplication>()
            .HasIndex(application => new
            {
                application.ClubTryoutId,
                application.PlayerProfileId
            })
            .IsUnique();

        builder.Entity<TryoutApplication>()
            .HasOne(application => application.ClubTryout)
            .WithMany(tryout => tryout.Applications)
            .HasForeignKey(application => application.ClubTryoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TryoutApplication>()
            .HasOne(application => application.PlayerProfile)
            .WithMany(profile => profile.TryoutApplications)
            .HasForeignKey(application => application.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasOne(notification => notification.User)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        ConfigureEnumConversions(builder);
    }

    private static void ConfigureEnumConversions(ModelBuilder builder)
    {
        builder.Entity<PlayerProfile>().Property(profile => profile.City).HasConversion<string>();
        builder.Entity<PlayerProfile>().Property(profile => profile.DominantFoot).HasConversion<string>();
        builder.Entity<PlayerProfile>().Property(profile => profile.PrimaryPosition).HasConversion<string>();
        builder.Entity<PlayerProfile>().Property(profile => profile.SecondaryPosition).HasConversion<string>();
        builder.Entity<Club>().Property(club => club.City).HasConversion<string>();
        builder.Entity<Club>().Property(club => club.ApprovalStatus).HasConversion<string>();
        builder.Entity<ClubMembership>().Property(membership => membership.Status).HasConversion<string>();
        builder.Entity<FootballMatch>().Property(match => match.Status).HasConversion<string>();
        builder.Entity<ScoutingReport>().Property(report => report.RecommendedPosition).HasConversion<string>();
        builder.Entity<ScoutingReport>().Property(report => report.Recommendation).HasConversion<string>();
        builder.Entity<ScoutingReport>().Property(report => report.Visibility).HasConversion<string>();
        builder.Entity<WatchlistItem>().Property(item => item.Status).HasConversion<string>();
        builder.Entity<CorrectionRequest>().Property(request => request.Status).HasConversion<string>();
        builder.Entity<Competition>().Property(competition => competition.Status).HasConversion<string>();
        builder.Entity<ClubTryout>().Property(tryout => tryout.Position).HasConversion<string>();
        builder.Entity<ClubTryout>().Property(tryout => tryout.Status).HasConversion<string>();
        builder.Entity<TryoutApplication>().Property(application => application.Status)
            .HasConversion<string>();
    }
}
