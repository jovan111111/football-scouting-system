using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using ScoutBoard.Api.Models;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Tests;

public class JwtServiceTests
{
    [Fact]
    public void CreateToken_ContainsUserAndRoleClaims()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Test-Key-Longer-Than-Thirty-Two-Characters-For-ScoutBoard",
                ["Jwt:Issuer"] = "ScoutBoard.Tests",
                ["Jwt:Audience"] = "ScoutBoard.Tests.Client",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
        var service = new JwtService(configuration);
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "igrac@scoutboard.test",
            FirstName = "Test",
            LastName = "Igrač"
        };

        var (token, expiresAt) = service.CreateToken(user, UserRoles.Player);
        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(parsedToken.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id);
        Assert.Contains(parsedToken.Claims, claim =>
            claim.Type == ClaimTypes.Role && claim.Value == UserRoles.Player);
        Assert.True(expiresAt > DateTime.UtcNow.AddMinutes(55));
    }
}
