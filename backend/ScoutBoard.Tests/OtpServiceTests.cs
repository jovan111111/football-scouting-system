using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.Models;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Tests;

public class OtpServiceTests
{
    [Fact]
    public async Task SendAndVerifyCode_WithCorrectCode_ConfirmsEmail()
    {
        using var infrastructure = await TestInfrastructure.CreateAsync();
        using var scope = infrastructure.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var otpService = scope.ServiceProvider.GetRequiredService<IOtpService>();
        var emailService = scope.ServiceProvider.GetRequiredService<CapturingEmailService>();

        var user = await CreateUser(userManager, "otp-success@scoutboard.test");
        await otpService.SendCodeAsync(user);
        var result = await otpService.VerifyCodeAsync(user, emailService.LastCode!);

        Assert.True(result.Success);
        var updatedUser = await userManager.FindByIdAsync(user.Id);
        Assert.True(updatedUser!.EmailConfirmed);
        Assert.Equal(user.Email, emailService.LastEmail);
    }

    [Fact]
    public async Task VerifyCode_WithWrongCode_IncreasesAttemptCount()
    {
        using var infrastructure = await TestInfrastructure.CreateAsync();
        using var scope = infrastructure.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var otpService = scope.ServiceProvider.GetRequiredService<IOtpService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await CreateUser(userManager, "otp-failure@scoutboard.test");
        await otpService.SendCodeAsync(user);
        var result = await otpService.VerifyCodeAsync(user, "999999");

        Assert.False(result.Success);
        Assert.False(user.EmailConfirmed);
        var code = await dbContext.EmailVerificationCodes.SingleAsync();
        Assert.Equal(1, code.AttemptCount);
        Assert.False(code.IsUsed);
    }

    private static async Task<ApplicationUser> CreateUser(
        UserManager<ApplicationUser> userManager,
        string email)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "Igrač"
        };
        var result = await userManager.CreateAsync(user, "Test1234");
        Assert.True(result.Succeeded);
        return user;
    }
}
