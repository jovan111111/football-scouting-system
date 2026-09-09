using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.Services;

public record ServiceResult<T>(bool Success, string Message, T? Data = default);

public interface IAuthService
{
    Task<ServiceResult<object>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<object>> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ServiceResult<object>> ResendOtpAsync(ResendOtpRequest request);
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<CurrentUserDto?> GetCurrentUserAsync(string userId);
}

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IOtpService otpService,
    IJwtService jwtService) : IAuthService
{
    public async Task<ServiceResult<object>> RegisterAsync(RegisterRequest request)
    {
        var role = request.Role.Trim();
        if (role is not (UserRoles.Player or UserRoles.CoachScout))
        {
            return new ServiceResult<object>(
                false,
                "Dozvoljene uloge pri registraciji su igrač i trener/skaut.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return new ServiceResult<object>(false, "Korisnik sa ovom e-mail adresom već postoji.");
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(error => error.Description));
            return new ServiceResult<object>(false, message);
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new ServiceResult<object>(false, "Nije moguće dodeliti izabranu ulogu.");
        }

        if (role == UserRoles.Player)
        {
            dbContext.PlayerProfiles.Add(new PlayerProfile { UserId = user.Id });
            await dbContext.SaveChangesAsync();
        }

        await otpService.SendCodeAsync(user);
        return new ServiceResult<object>(
            true,
            "Registracija je uspešna. Kod za potvrdu poslat je na e-mail adresu.");
    }

    public async Task<ServiceResult<object>> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return new ServiceResult<object>(false, "Korisnik nije pronađen.");
        }

        if (user.EmailConfirmed)
        {
            return new ServiceResult<object>(true, "E-mail adresa je već potvrđena.");
        }

        var result = await otpService.VerifyCodeAsync(user, request.Code);
        return new ServiceResult<object>(result.Success, result.Message);
    }

    public async Task<ServiceResult<object>> ResendOtpAsync(ResendOtpRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return new ServiceResult<object>(true, "Ako nalog postoji, novi kod je poslat.");
        }

        if (user.EmailConfirmed)
        {
            return new ServiceResult<object>(false, "E-mail adresa je već potvrđena.");
        }

        await otpService.SendCodeAsync(user);
        return new ServiceResult<object>(true, "Novi kod je poslat.");
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return new ServiceResult<LoginResponse>(false, "E-mail ili lozinka nisu ispravni.");
        }

        if (!user.IsActive)
        {
            return new ServiceResult<LoginResponse>(false, "Korisnički nalog je deaktiviran.");
        }

        if (!user.EmailConfirmed)
        {
            return new ServiceResult<LoginResponse>(false, "E-mail adresa nije potvrđena.");
        }

        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
        var playerProfileId = await dbContext.PlayerProfiles
            .Where(profile => profile.UserId == user.Id)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        var (token, expiresAt) = jwtService.CreateToken(user, role);
        var currentUser = new CurrentUserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            role,
            playerProfileId);

        return new ServiceResult<LoginResponse>(
            true,
            "Prijava je uspešna.",
            new LoginResponse(token, expiresAt, currentUser));
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
        var playerProfileId = await dbContext.PlayerProfiles
            .Where(profile => profile.UserId == user.Id)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        return new CurrentUserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            role,
            playerProfileId);
    }
}
