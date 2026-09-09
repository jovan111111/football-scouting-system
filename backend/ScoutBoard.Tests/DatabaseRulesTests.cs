using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Tests;

public class DatabaseRulesTests
{
    [Fact]
    public async Task Watchlist_DoesNotAllowSamePlayerTwiceForSameCoach()
    {
        using var infrastructure = await TestInfrastructure.CreateAsync();
        using var scope = infrastructure.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var coach = new ApplicationUser
        {
            Id = "coach-1",
            UserName = "coach@test.local",
            NormalizedUserName = "COACH@TEST.LOCAL",
            Email = "coach@test.local",
            NormalizedEmail = "COACH@TEST.LOCAL",
            FirstName = "Test",
            LastName = "Trener"
        };
        var playerUser = new ApplicationUser
        {
            Id = "player-1",
            UserName = "player@test.local",
            NormalizedUserName = "PLAYER@TEST.LOCAL",
            Email = "player@test.local",
            NormalizedEmail = "PLAYER@TEST.LOCAL",
            FirstName = "Test",
            LastName = "Igrač"
        };
        var player = new PlayerProfile { User = playerUser };
        dbContext.Users.Add(coach);
        dbContext.PlayerProfiles.Add(player);
        await dbContext.SaveChangesAsync();

        dbContext.WatchlistItems.Add(new WatchlistItem
        {
            CoachScoutId = coach.Id,
            PlayerProfileId = player.Id
        });
        await dbContext.SaveChangesAsync();

        dbContext.WatchlistItems.Add(new WatchlistItem
        {
            CoachScoutId = coach.Id,
            PlayerProfileId = player.Id
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }
}
