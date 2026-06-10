using Microsoft.EntityFrameworkCore;
using SetlistSocial.Api.Data;
using SetlistSocial.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    options.UseSqlite(connectionString);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithTags("Health");

app.MapGet("/api/public/stats", async (AppDbContext db) =>
{
    var stats = new
    {
        users = await db.UserProfiles.CountAsync(),
        artists = await db.Artists.CountAsync(),
        concerts = await db.Concerts.CountAsync(),
        reviews = await db.Reviews.CountAsync(),
        wishlistItems = await db.WishlistItems.CountAsync(),
        activityEvents = await db.ActivityEvents.CountAsync(),
        tags = await db.Tags.CountAsync()
    };

    return Results.Ok(stats);
})
    .WithName("PublicStats")
    .WithTags("Public");

if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/dev/seed", async (AppDbContext db) =>
    {
        if (await db.UserProfiles.AnyAsync())
        {
            return Results.Ok(new { status = "already-seeded" });
        }

        var now = DateTimeOffset.UtcNow;

        var indieTag = new Tag
        {
            Name = "Indie",
            CreatedAt = now,
            UpdatedAt = now
        };

        var liveTag = new Tag
        {
            Name = "Live Favorite",
            CreatedAt = now,
            UpdatedAt = now
        };

        var user = new UserProfile
        {
            DisplayName = "Demo Listener",
            OAuthSubject = "dev-google-oauth-subject-placeholder",
            CreatedAt = now,
            UpdatedAt = now
        };

        var artist = new Artist
        {
            Name = "The Example Set",
            CreatedAt = now,
            UpdatedAt = now
        };

        var concert = new Concert
        {
            Title = "The Example Set at Local Hall",
            VenueName = "Local Hall",
            City = "Chicago",
            Region = "IL",
            Country = "USA",
            ConcertDate = now.Date,
            UserProfile = user,
            Artist = artist,
            Tags = [indieTag, liveTag],
            CreatedAt = now,
            UpdatedAt = now
        };

        var review = new Review
        {
            Rating = 5,
            Body = "Tiny seed review for local development.",
            UserProfile = user,
            Concert = concert,
            CreatedAt = now,
            UpdatedAt = now
        };

        var wishlistItem = new WishlistItem
        {
            Notes = "Catch the next tour.",
            UserProfile = user,
            Artist = artist,
            CreatedAt = now,
            UpdatedAt = now
        };

        var activityEvent = new ActivityEvent
        {
            EventType = "ConcertReviewed",
            Summary = "Demo Listener reviewed The Example Set at Local Hall.",
            UserProfile = user,
            Concert = concert,
            CreatedAt = now
        };

        db.AddRange(user, artist, concert, review, wishlistItem, activityEvent);
        await db.SaveChangesAsync();

        return Results.Created("/api/public/stats", new { status = "seeded" });
    })
        .WithName("SeedDevelopmentData")
        .WithTags("Development");
}

app.Run();
