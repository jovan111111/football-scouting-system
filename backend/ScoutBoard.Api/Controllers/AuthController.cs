using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoutBoard.Api.DTOs;
using ScoutBoard.Api.Helpers;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, new MessageResponse(result.Message))
            : BadRequest(new MessageResponse(result.Message));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        var result = await authService.VerifyEmailAsync(request);
        return result.Success
            ? Ok(new MessageResponse(result.Message))
            : BadRequest(new MessageResponse(result.Message));
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest request)
    {
        var result = await authService.ResendOtpAsync(request);
        return result.Success
            ? Ok(new MessageResponse(result.Message))
            : BadRequest(new MessageResponse(result.Message));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return result.Success
            ? Ok(result.Data)
            : Unauthorized(new MessageResponse(result.Message));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized(new MessageResponse("Prijava nije važeća."));
        }

        var user = await authService.GetCurrentUserAsync(userId);
        return user is null
            ? NotFound(new MessageResponse("Korisnik nije pronađen."))
            : Ok(user);
    }
}
