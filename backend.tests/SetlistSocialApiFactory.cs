using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SetlistSocial.Api.Data;
using SetlistSocial.Api.External;
using SetlistSocial.Api.Models;

namespace SetlistSocial.Api.Tests;

public sealed class SetlistSocialApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Google:ClientId"] = "test-client-id",
                ["Google:ClientSecret"] = "test-client-secret",
                ["FrontendUrl"] = "http://localhost:5173",
                ["LastFm:ApiKey"] = "test-lastfm-api-key"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<ILastFmClient>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            services.AddSingleton<ILastFmClient, StubLastFmClient>();

            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultSignInScheme = TestAuthHandler.SchemeName;
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public async Task SeedUserAsync(string oauthSubject, string displayName)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.UserProfiles.AnyAsync(user => user.OAuthSubject == oauthSubject))
        {
            return;
        }

        db.UserProfiles.Add(new UserProfile
        {
            OAuthSubject = oauthSubject,
            DisplayName = displayName
        });

        await db.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

file sealed class StubLastFmClient : ILastFmClient
{
    public Task<IReadOnlyList<LastFmArtistSearchResult>> SearchArtistsAsync(
        string artistName,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<LastFmArtistSearchResult> results =
        [
            new LastFmArtistSearchResult(
                $"{artistName.Trim()} Test Result",
                "https://www.last.fm/music/test-result",
                12345,
                "https://example.com/test-result.png")
        ];

        return Task.FromResult(results);
    }
}
