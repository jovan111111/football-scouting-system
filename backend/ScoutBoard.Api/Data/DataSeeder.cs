using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in UserRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (!string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            await EnsureUserAsync(
                userManager,
                adminEmail,
                adminPassword,
                "ScoutBoard",
                "Administrator",
                UserRoles.Admin);
        }

        if (configuration.GetValue<bool>("Seed:DemoData"))
        {
            await SeedDemoDataAsync(dbContext, userManager);
        }
    }

    private static async Task SeedDemoDataAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var coach = await EnsureUserAsync(
            userManager,
            "trener@scoutboard.rs",
            "Trener123!",
            "Adnan",
            "Kolašinac",
            UserRoles.CoachScout);

        var playerOne = await EnsurePlayerAsync(
            dbContext,
            userManager,
            "igrac1@scoutboard.rs",
            "Igrac123!",
            "Emir",
            "Hadžić",
            City.NoviPazar,
            FootballPosition.Striker,
            184,
            true);

        var playerTwo = await EnsurePlayerAsync(
            dbContext,
            userManager,
            "igrac2@scoutboard.rs",
            "Igrac123!",
            "Hamza",
            "Muratović",
            City.Tutin,
            FootballPosition.CentralMidfielder,
            178,
            false);

        var playerThree = await EnsurePlayerAsync(
            dbContext,
            userManager,
            "igrac3@scoutboard.rs",
            "Igrac123!",
            "Tarik",
            "Bihorac",
            City.Sjenica,
            FootballPosition.CentreBack,
            188,
            true);

        var club = await dbContext.Clubs
            .FirstOrDefaultAsync(item => item.Name == "FK Dolina");
        if (club is null)
        {
            club = new Club
            {
                Name = "FK Dolina",
                City = City.NoviPazar,
                FoundedYear = 2016,
                Description = "Razvojni amaterski klub posvećen mladim igračima iz Novog Pazara i okoline.",
                StadiumName = "Gradski pomoćni teren",
                Address = "Novi Pazar",
                OwnerId = coach.Id,
                ApprovalStatus = ApprovalStatus.Approved
            };

            dbContext.Clubs.Add(club);
            await dbContext.SaveChangesAsync();
        }

        var opponentClub = await dbContext.Clubs
            .FirstOrDefaultAsync(item => item.Name == "FK Pešter");
        if (opponentClub is null)
        {
            opponentClub = new Club
            {
                Name = "FK Pešter",
                City = City.Sjenica,
                FoundedYear = 2014,
                Description = "Amaterski klub sa Pešterske visoravni.",
                StadiumName = "Sportski teren Pešter",
                Address = "Sjenica",
                OwnerId = coach.Id,
                ApprovalStatus = ApprovalStatus.Approved
            };
            dbContext.Clubs.Add(opponentClub);
            await dbContext.SaveChangesAsync();
        }

        foreach (var playerId in new[] { playerOne.Id, playerTwo.Id, playerThree.Id })
        {
            if (!await dbContext.ClubMemberships.AnyAsync(item =>
                    item.ClubId == club.Id &&
                    item.PlayerProfileId == playerId &&
                    item.Status == MembershipStatus.Active))
            {
                dbContext.ClubMemberships.Add(CreateActiveMembership(club.Id, playerId));
            }
        }
        await dbContext.SaveChangesAsync();

        var competition = await dbContext.Competitions
            .Include(item => item.Participants)
            .FirstOrDefaultAsync(item =>
                item.Name == "Sandžačka amaterska liga" &&
                item.Season == "2026");
        if (competition is null)
        {
            competition = new Competition
            {
                Name = "Sandžačka amaterska liga",
                Season = "2026",
                Description = "Lokalno demonstraciono takmičenje klubova iz Novog Pazara i okoline.",
                StartDate = DateTime.UtcNow.Date.AddMonths(-2),
                EndDate = DateTime.UtcNow.Date.AddMonths(4),
                Status = CompetitionStatus.Active,
                CreatedById = coach.Id
            };
            dbContext.Competitions.Add(competition);
            await dbContext.SaveChangesAsync();
        }

        foreach (var participantClubId in new[] { club.Id, opponentClub.Id })
        {
            if (!competition.Participants.Any(entry => entry.ClubId == participantClubId))
            {
                competition.Participants.Add(new CompetitionClub
                {
                    ClubId = participantClubId
                });
            }
        }
        await dbContext.SaveChangesAsync();

        var footballMatch = await dbContext.FootballMatches
            .Include(item => item.PlayerStatistics)
            .FirstOrDefaultAsync(item =>
                item.ClubId == club.Id &&
                item.MatchReport == "Dobra timska utakmica i pobeda domaćeg kluba.");
        if (footballMatch is null)
        {
            footballMatch = new FootballMatch
            {
                ClubId = club.Id,
                MatchDate = DateTime.UtcNow.AddDays(-7),
                Venue = "Gradski pomoćni teren",
                IsHomeMatch = true,
                GoalsScored = 2,
                GoalsConceded = 1,
                Status = FootballMatchStatus.Completed,
                MatchReport = "Dobra timska utakmica i pobeda domaćeg kluba."
            };
            dbContext.FootballMatches.Add(footballMatch);
        }

        footballMatch.OpponentClubId = opponentClub.Id;
        footballMatch.OpponentName = null;
        footballMatch.CompetitionId = competition.Id;
        await dbContext.SaveChangesAsync();

        var demoStatistics = new[]
        {
            new PlayerMatchStatistic
            {
                MatchId = footballMatch.Id,
                PlayerProfileId = playerOne.Id,
                WasStarter = true,
                MinutesPlayed = 90,
                Goals = 1,
                Assists = 1,
                Rating = 8.4m
            },
            new PlayerMatchStatistic
            {
                MatchId = footballMatch.Id,
                PlayerProfileId = playerTwo.Id,
                WasStarter = true,
                MinutesPlayed = 82,
                Goals = 1,
                Rating = 7.8m
            },
            new PlayerMatchStatistic
            {
                MatchId = footballMatch.Id,
                PlayerProfileId = playerThree.Id,
                WasStarter = true,
                MinutesPlayed = 90,
                YellowCards = 1,
                Rating = 7.2m
            }
        };

        foreach (var demoStatistic in demoStatistics)
        {
            var existingStatistic = footballMatch.PlayerStatistics.FirstOrDefault(item =>
                item.PlayerProfileId == demoStatistic.PlayerProfileId);
            if (existingStatistic is null)
            {
                dbContext.PlayerMatchStatistics.Add(demoStatistic);
            }
            else
            {
                existingStatistic.Rating = demoStatistic.Rating;
            }
        }

        if (!await dbContext.ScoutingReports.AnyAsync(item =>
                item.AuthorId == coach.Id &&
                item.PlayerProfileId == playerOne.Id))
        {
            dbContext.ScoutingReports.Add(new ScoutingReport
            {
                AuthorId = coach.Id,
                PlayerProfileId = playerOne.Id,
                Technique = 8,
                Speed = 8,
                Passing = 7,
                Shooting = 8,
                Defending = 4,
                PhysicalCondition = 8,
                GameVision = 7,
                Teamwork = 8,
                Strengths = "Kretanje bez lopte i završnica.",
                Weaknesses = "Igra slabijom nogom.",
                Comment = "Pouzdan napadač sa dobrim osećajem za prostor.",
                RecommendedPosition = FootballPosition.Striker,
                Recommendation = RecommendationLevel.Recommended,
                Visibility = ReportVisibility.Public
            });
        }

        if (!await dbContext.ClubTryouts.AnyAsync(item => item.ClubId == club.Id))
        {
            dbContext.ClubTryouts.Add(new ClubTryout
            {
                ClubId = club.Id,
                Title = "Otvorena proba za seniorski tim",
                Description = "Klub traži igrače za dopunu seniorske ekipe. Poneti sportsku opremu.",
                TryoutDate = DateTime.UtcNow.AddDays(21),
                Venue = "Gradski pomoćni teren",
                MinimumAge = 17,
                MaximumAge = 28,
                Status = TryoutStatus.Open
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static ClubMembership CreateActiveMembership(int clubId, int playerProfileId) =>
        new()
        {
            ClubId = clubId,
            PlayerProfileId = playerProfileId,
            Status = MembershipStatus.Active,
            RespondedAt = DateTime.UtcNow.AddMonths(-2),
            JoinedAt = DateTime.UtcNow.AddMonths(-2)
        };

    private static async Task<PlayerProfile> EnsurePlayerAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        City city,
        FootballPosition position,
        int height,
        bool lookingForClub)
    {
        var user = await EnsureUserAsync(
            userManager,
            email,
            password,
            firstName,
            lastName,
            UserRoles.Player);

        var profile = await dbContext.PlayerProfiles
            .FirstOrDefaultAsync(item => item.UserId == user.Id);

        if (profile is null)
        {
            profile = new PlayerProfile { UserId = user.Id };
            dbContext.PlayerProfiles.Add(profile);
        }

        profile.City = city;
        profile.PrimaryPosition = position;
        profile.DominantFoot = DominantFoot.Right;
        profile.HeightCm = height;
        profile.DateOfBirth = new DateTime(2002, 1, 1);
        profile.LookingForClub = lookingForClub;
        profile.Biography = "Demonstracioni profil igrača za ScoutBoard MVP.";

        await dbContext.SaveChangesAsync();
        return profile;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Nije moguće kreirati početnog korisnika {email}: " +
                string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
