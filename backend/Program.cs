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
        var hasExistingData =
            await db.UserProfiles.AnyAsync()
            || await db.Artists.AnyAsync()
            || await db.Concerts.AnyAsync()
            || await db.Reviews.AnyAsync()
            || await db.WishlistItems.AnyAsync()
            || await db.ActivityEvents.AnyAsync()
            || await db.Tags.AnyAsync();

        if (hasExistingData)
        {
            return Results.Ok(new { status = "already-seeded" });
        }

        var now = DateTimeOffset.UtcNow;

        var tags = new[]
        {
            new Tag { Name = "Indie", CreatedAt = now, UpdatedAt = now },
            new Tag { Name = "Arena", CreatedAt = now, UpdatedAt = now },
            new Tag { Name = "Festival", CreatedAt = now, UpdatedAt = now },
            new Tag { Name = "Acoustic", CreatedAt = now, UpdatedAt = now },
            new Tag { Name = "Local Favorite", CreatedAt = now, UpdatedAt = now },
            new Tag { Name = "Encore Worthy", CreatedAt = now, UpdatedAt = now }
        };

        var users = new[]
        {
            new UserProfile
            {
                DisplayName = "Maya Rivera",
                OAuthSubject = "dev-google-oauth-subject-maya",
                Bio = "Keeps track of memorable live sets.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new UserProfile
            {
                DisplayName = "Jordan Lee",
                OAuthSubject = "dev-google-oauth-subject-jordan",
                Bio = "Always hunting for the next great opener.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new UserProfile
            {
                DisplayName = "Sam Carter",
                OAuthSubject = "dev-google-oauth-subject-sam",
                Bio = "Likes small venues and loud encores.",
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        var artists = new[]
        {
            new Artist { Name = "The Example Set", CreatedAt = now, UpdatedAt = now },
            new Artist { Name = "Neon Wednesdays", CreatedAt = now, UpdatedAt = now },
            new Artist { Name = "Lakefront Static", CreatedAt = now, UpdatedAt = now },
            new Artist { Name = "The Midnight Reprise", CreatedAt = now, UpdatedAt = now },
            new Artist { Name = "North Loop Choir", CreatedAt = now, UpdatedAt = now }
        };

        var concerts = new[]
        {
            new Concert
            {
                Title = "The Example Set at Local Hall",
                VenueName = "Local Hall",
                City = "Chicago",
                Region = "IL",
                Country = "USA",
                ConcertDate = now.AddDays(-18),
                UserProfile = users[0],
                Artist = artists[0],
                Tags = [tags[0], tags[4], tags[5]],
                CreatedAt = now,
                UpdatedAt = now
            },
            new Concert
            {
                Title = "Neon Wednesdays Summer Show",
                VenueName = "Metro Room",
                City = "Chicago",
                Region = "IL",
                Country = "USA",
                ConcertDate = now.AddDays(-10),
                UserProfile = users[1],
                Artist = artists[1],
                Tags = [tags[0], tags[1]],
                CreatedAt = now,
                UpdatedAt = now
            },
            new Concert
            {
                Title = "Lakefront Static Festival Set",
                VenueName = "Grant Park",
                City = "Chicago",
                Region = "IL",
                Country = "USA",
                ConcertDate = now.AddDays(-7),
                UserProfile = users[2],
                Artist = artists[2],
                Tags = [tags[2], tags[5]],
                CreatedAt = now,
                UpdatedAt = now
            },
            new Concert
            {
                Title = "The Midnight Reprise Late Show",
                VenueName = "Blue Door Theater",
                City = "Milwaukee",
                Region = "WI",
                Country = "USA",
                ConcertDate = now.AddDays(-3),
                UserProfile = users[0],
                Artist = artists[3],
                Tags = [tags[1], tags[5]],
                CreatedAt = now,
                UpdatedAt = now
            },
            new Concert
            {
                Title = "North Loop Choir Acoustic Night",
                VenueName = "River Room",
                City = "Minneapolis",
                Region = "MN",
                Country = "USA",
                ConcertDate = now.AddDays(14),
                UserProfile = users[1],
                Artist = artists[4],
                Tags = [tags[3], tags[4]],
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        var reviews = new[]
        {
            new Review { Rating = 5, Body = "The encore made the whole room light up.", UserProfile = users[0], Concert = concerts[0], CreatedAt = now, UpdatedAt = now },
            new Review { Rating = 4, Body = "Great pacing and a strong opener.", UserProfile = users[1], Concert = concerts[1], CreatedAt = now, UpdatedAt = now },
            new Review { Rating = 5, Body = "Perfect festival slot with huge crowd energy.", UserProfile = users[2], Concert = concerts[2], CreatedAt = now, UpdatedAt = now },
            new Review { Rating = 4, Body = "Late show was worth staying up for.", UserProfile = users[0], Concert = concerts[3], CreatedAt = now, UpdatedAt = now },
            new Review { Rating = 3, Body = "Looking forward to seeing how this acoustic set lands live.", UserProfile = users[1], Concert = concerts[4], CreatedAt = now, UpdatedAt = now }
        };

        var wishlistItems = new[]
        {
            new WishlistItem { Notes = "See Neon Wednesdays again this fall.", UserProfile = users[0], Artist = artists[1], CreatedAt = now, UpdatedAt = now },
            new WishlistItem { Notes = "Catch Lakefront Static outside festival season.", UserProfile = users[1], Artist = artists[2], CreatedAt = now, UpdatedAt = now },
            new WishlistItem { Notes = "Find a smaller North Loop Choir show.", UserProfile = users[2], Artist = artists[4], CreatedAt = now, UpdatedAt = now }
        };

        var activityEvents = new[]
        {
            new ActivityEvent { EventType = "UserJoined", Summary = "Maya Rivera joined Setlist Social.", UserProfile = users[0], CreatedAt = now },
            new ActivityEvent { EventType = "UserJoined", Summary = "Jordan Lee joined Setlist Social.", UserProfile = users[1], CreatedAt = now },
            new ActivityEvent { EventType = "UserJoined", Summary = "Sam Carter joined Setlist Social.", UserProfile = users[2], CreatedAt = now },
            new ActivityEvent { EventType = "ConcertAdded", Summary = "Maya added The Example Set at Local Hall.", UserProfile = users[0], Concert = concerts[0], CreatedAt = now },
            new ActivityEvent { EventType = "ConcertAdded", Summary = "Jordan added Neon Wednesdays Summer Show.", UserProfile = users[1], Concert = concerts[1], CreatedAt = now },
            new ActivityEvent { EventType = "ConcertReviewed", Summary = "Sam reviewed Lakefront Static Festival Set.", UserProfile = users[2], Concert = concerts[2], CreatedAt = now },
            new ActivityEvent { EventType = "WishlistItemAdded", Summary = "Maya added Neon Wednesdays to a wishlist.", UserProfile = users[0], CreatedAt = now },
            new ActivityEvent { EventType = "ConcertReviewed", Summary = "Maya reviewed The Midnight Reprise Late Show.", UserProfile = users[0], Concert = concerts[3], CreatedAt = now }
        };

        db.AddRange(tags);
        db.AddRange(users);
        db.AddRange(artists);
        db.AddRange(concerts);
        db.AddRange(reviews);
        db.AddRange(wishlistItems);
        db.AddRange(activityEvents);
        await db.SaveChangesAsync();

        return Results.Created("/api/public/stats", new
        {
            status = "seeded",
            users = users.Length,
            artists = artists.Length,
            concerts = concerts.Length,
            reviews = reviews.Length,
            wishlistItems = wishlistItems.Length,
            activityEvents = activityEvents.Length,
            tags = tags.Length
        });
    })
        .WithName("SeedDevelopmentData")
        .WithTags("Development");
}

app.Run();
