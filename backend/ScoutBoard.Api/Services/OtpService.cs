using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Services;

public record OtpResult(bool Success, string Message);

public interface IOtpService
{
    Task SendCodeAsync(ApplicationUser user);
    Task<OtpResult> VerifyCodeAsync(ApplicationUser user, string code);
}

public class OtpService(
    ApplicationDbContext dbContext,
    IEmailService emailService,
    UserManager<ApplicationUser> userManager) : IOtpService
{
    private const int ExpirationMinutes = 10;
    private const int MaximumAttempts = 5;
    private readonly PasswordHasher<EmailVerificationCode> _hasher = new();

    public async Task SendCodeAsync(ApplicationUser user)
    {
        var existingCodes = await dbContext.EmailVerificationCodes
            .Where(item => item.UserId == user.Id && !item.IsUsed)
            .ToListAsync();

        foreach (var existingCode in existingCodes)
        {
            existingCode.IsUsed = true;
        }

        var plainCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var verificationCode = new EmailVerificationCode
        {
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes)
        };
        verificationCode.CodeHash = _hasher.HashPassword(verificationCode, plainCode);

        dbContext.EmailVerificationCodes.Add(verificationCode);
        await dbContext.SaveChangesAsync();

        await emailService.SendVerificationCodeAsync(
            user.Email ?? throw new InvalidOperationException("Korisnik nema e-mail adresu."),
            user.FirstName,
            plainCode);
    }

    public async Task<OtpResult> VerifyCodeAsync(ApplicationUser user, string code)
    {
        var verificationCode = await dbContext.EmailVerificationCodes
            .Where(item => item.UserId == user.Id && !item.IsUsed)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync();

        if (verificationCode is null)
        {
            return new OtpResult(false, "Aktivan kod za potvrdu nije pronađen.");
        }

        if (verificationCode.ExpiresAt <= DateTime.UtcNow)
        {
            verificationCode.IsUsed = true;
            await dbContext.SaveChangesAsync();
            return new OtpResult(false, "Kod je istekao. Zatražite novi kod.");
        }

        if (verificationCode.AttemptCount >= MaximumAttempts)
        {
            verificationCode.IsUsed = true;
            await dbContext.SaveChangesAsync();
            return new OtpResult(false, "Prekoračen je dozvoljen broj pokušaja.");
        }

        verificationCode.AttemptCount++;
        var result = _hasher.VerifyHashedPassword(
            verificationCode,
            verificationCode.CodeHash,
            code);

        if (result == PasswordVerificationResult.Failed)
        {
            await dbContext.SaveChangesAsync();
            return new OtpResult(false, "Uneti kod nije ispravan.");
        }

        verificationCode.IsUsed = true;
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
        await dbContext.SaveChangesAsync();

        return new OtpResult(true, "E-mail adresa je uspešno potvrđena.");
    }
}
