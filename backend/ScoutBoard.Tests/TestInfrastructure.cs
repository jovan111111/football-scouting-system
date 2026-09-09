using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoutBoard.Api.Data;
using ScoutBoard.Api.Models;
using ScoutBoard.Api.Services;

namespace ScoutBoard.Tests;

internal sealed class CapturingEmailService : IEmailService
{
    public string? LastEmail { get; private set; }
    public string? LastCode { get; private set; }

    public Task SendVerificationCodeAsync(string email, string firstName, string code)
    {
        LastEmail = email;
        LastCode = code;
        return Task.CompletedTask;
    }
}

internal sealed class TestInfrastructure : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    private TestInfrastructure(SqliteConnection connection, ServiceProvider provider)
    {
        _connection = connection;
        _provider = provider;
    }

    public IServiceScope CreateScope() => _provider.CreateScope();

    public static async Task<TestInfrastructure> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddSingleton<CapturingEmailService>();
        services.AddSingleton<IEmailService>(provider =>
            provider.GetRequiredService<CapturingEmailService>());
        services.AddScoped<IOtpService, OtpService>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Database.EnsureCreatedAsync();

        return new TestInfrastructure(connection, provider);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
