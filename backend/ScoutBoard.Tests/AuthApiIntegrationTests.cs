using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Tests;

public class AuthApiIntegrationTests
{
    [Fact]
    public async Task Registration_Otp_Jwt_And_Protected_Api_Work_Together()
    {
        await using var factory = new ScoutBoardApiFactory();
        using var client = factory.CreateClient();

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "OTP",
            lastName = "Provera",
            email = "otp-provera@scoutboard.test",
            password = "Provera123!",
            role = "CoachScout"
        });

        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var loginBeforeVerification = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "otp-provera@scoutboard.test",
            password = "Provera123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginBeforeVerification.StatusCode);

        var emailService = factory.Services.GetRequiredService<CapturingEmailService>();
        Assert.Equal("otp-provera@scoutboard.test", emailService.LastEmail);
        Assert.Matches(@"^\d{6}$", emailService.LastCode);

        var verification = await client.PostAsJsonAsync("/api/auth/verify-email", new
        {
            email = "otp-provera@scoutboard.test",
            code = emailService.LastCode
        });

        Assert.Equal(HttpStatusCode.OK, verification.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "otp-provera@scoutboard.test",
            password = "Provera123!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var loginDocument = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginDocument.RootElement.GetProperty("token").GetString();
        Assert.NotNull(token);
        Assert.Equal(3, token.Split('.').Length);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var protectedResponse = await client.GetAsync("/api/auth/me");
        Assert.True(
            protectedResponse.StatusCode == HttpStatusCode.OK,
            $"Status: {protectedResponse.StatusCode}; " +
            $"WWW-Authenticate: {string.Join(", ", protectedResponse.Headers.WwwAuthenticate)}; " +
            $"Body: {await protectedResponse.Content.ReadAsStringAsync()}");

        using var userDocument =
            JsonDocument.Parse(await protectedResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "otp-provera@scoutboard.test",
            userDocument.RootElement.GetProperty("email").GetString());
        Assert.Equal(
            "CoachScout",
            userDocument.RootElement.GetProperty("role").GetString());

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "neispravan.token.vrednost");

        var invalidTokenResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, invalidTokenResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var publicApiResponse = await client.GetAsync("/api/players");
        Assert.Equal(HttpStatusCode.OK, publicApiResponse.StatusCode);
    }
}

internal sealed class ScoutBoardApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:DemoData"] = "false",
                ["Seed:AdminEmail"] = "",
                ["Seed:AdminPassword"] = ""
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlite(_connection));

            services.RemoveAll<IEmailService>();
            services.AddSingleton<CapturingEmailService>();
            services.AddSingleton<IEmailService>(
                provider => provider.GetRequiredService<CapturingEmailService>());
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
