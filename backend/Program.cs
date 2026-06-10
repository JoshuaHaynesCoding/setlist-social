using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SetlistSocial.Api.Data;
using SetlistSocial.Api.Models;

var builder = WebApplication.CreateBuilder(args);

const string DevelopmentCorsPolicy = "DevelopmentFrontend";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    options.UseSqlite(connectionString);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevelopmentCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "setlist_social_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? string.Empty;

        // Keep Google tokens out of the auth cookie; the app only needs identity claims for now.
        options.SaveTokens = false;

        // Google handles this internal callback, then redirects to /api/auth/callback.
        options.CallbackPath = "/api/auth/google-callback";
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevelopmentCorsPolicy);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithTags("Health");

app.MapGet("/api/auth/login", (IConfiguration configuration) =>
{
    var googleClientId = configuration["Google:ClientId"];
    var googleClientSecret = configuration["Google:ClientSecret"];

    if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
    {
        return Results.Problem(
            title: "Google OAuth is not configured.",
            detail: "Set Google__ClientId and Google__ClientSecret before starting the login flow.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/api/auth/callback" },
        [GoogleDefaults.AuthenticationScheme]);
})
    .WithName("AuthLogin")
    .WithTags("Auth");

app.MapGet("/api/auth/callback", async (
    ClaimsPrincipal principal,
    AppDbContext db,
    IConfiguration configuration) =>
{
    if (principal.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var oauthSubject = GetOAuthSubject(principal);

    if (oauthSubject is null)
    {
        return Results.Unauthorized();
    }

    var displayName = GetDisplayName(principal);
    var userProfile = await db.UserProfiles
        .SingleOrDefaultAsync(user => user.OAuthSubject == oauthSubject);

    if (userProfile is null)
    {
        userProfile = new UserProfile
        {
            DisplayName = displayName,
            OAuthSubject = oauthSubject
        };

        db.UserProfiles.Add(userProfile);
    }
    else
    {
        userProfile.DisplayName = displayName;
    }

    await db.SaveChangesAsync();

    var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:5173";
    return Results.Redirect(frontendUrl);
})
    .RequireAuthorization()
    .WithName("AuthCallback")
    .WithTags("Auth");

app.MapPost("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { status = "signed-out" });
})
    .WithName("AuthLogout")
    .WithTags("Auth");

app.MapGet("/api/me", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    if (principal.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var oauthSubject = GetOAuthSubject(principal);

    if (oauthSubject is null)
    {
        return Results.Unauthorized();
    }

    var userProfile = await db.UserProfiles
        .AsNoTracking()
        .Where(user => user.OAuthSubject == oauthSubject)
        .Select(user => new
        {
            user.Id,
            user.DisplayName,
            user.Bio,
            user.CreatedAt,
            user.UpdatedAt
        })
        .SingleOrDefaultAsync();

    return userProfile is null
        ? Results.Unauthorized()
        : Results.Ok(userProfile);
})
    .WithName("Me")
    .WithTags("Auth");

app.MapGet("/api/me/dashboard", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    if (principal.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var oauthSubject = GetOAuthSubject(principal);

    if (oauthSubject is null)
    {
        return Results.Unauthorized();
    }

    var userProfile = await db.UserProfiles
        .AsNoTracking()
        .Where(user => user.OAuthSubject == oauthSubject)
        .Select(user => new
        {
            user.Id,
            user.DisplayName,
            user.Bio,
            user.CreatedAt,
            user.UpdatedAt,
            ConcertCount = user.Concerts.Count,
            ReviewCount = user.Reviews.Count,
            WishlistItemCount = user.WishlistItems.Count,
            RecentActivityEventCount = user.ActivityEvents.Count
        })
        .SingleOrDefaultAsync();

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        Profile = new
        {
            userProfile.Id,
            userProfile.DisplayName,
            userProfile.Bio,
            userProfile.CreatedAt,
            userProfile.UpdatedAt
        },
        Counts = new
        {
            Concerts = userProfile.ConcertCount,
            Reviews = userProfile.ReviewCount,
            WishlistItems = userProfile.WishlistItemCount,
            RecentActivityEvents = userProfile.RecentActivityEventCount
        }
    });
})
    .WithName("MeDashboard")
    .WithTags("Auth");

app.MapGet("/api/me/concerts", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var concerts = await db.Concerts
        .AsNoTracking()
        .Where(concert => concert.UserProfileId == userProfile.Id)
        .Select(concert => new ConcertResponse(
            concert.Id,
            concert.Title,
            concert.Artist.Name,
            concert.VenueName,
            concert.City,
            concert.Region,
            concert.Country,
            concert.ConcertDate,
            concert.CreatedAt,
            concert.UpdatedAt))
        .ToListAsync();

    return Results.Ok(concerts
        .OrderByDescending(concert => concert.ConcertDate)
        .ToList());
})
    .WithName("MeConcerts")
    .WithTags("My Concerts");

app.MapGet("/api/me/concerts/{id:int}", async (int id, ClaimsPrincipal principal, AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var concert = await db.Concerts
        .AsNoTracking()
        .Where(concert => concert.Id == id && concert.UserProfileId == userProfile.Id)
        .Select(concert => new ConcertResponse(
            concert.Id,
            concert.Title,
            concert.Artist.Name,
            concert.VenueName,
            concert.City,
            concert.Region,
            concert.Country,
            concert.ConcertDate,
            concert.CreatedAt,
            concert.UpdatedAt))
        .SingleOrDefaultAsync();

    // Non-owned ids return 404 to avoid revealing whether another user's concert exists.
    return concert is null ? Results.NotFound() : Results.Ok(concert);
})
    .WithName("MeConcert")
    .WithTags("My Concerts");

app.MapPost("/api/me/concerts", async (ConcertRequest request, ClaimsPrincipal principal, AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var validationErrors = ValidateConcertRequest(request);

    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var artist = await FindOrCreateArtistAsync(request.ArtistName, db);

    var concert = new Concert
    {
        Title = request.Title.Trim(),
        VenueName = TrimToNull(request.VenueName),
        City = TrimToNull(request.City),
        Region = TrimToNull(request.Region),
        Country = TrimToNull(request.Country),
        ConcertDate = request.ConcertDate,
        Artist = artist,
        UserProfileId = userProfile.Id
    };

    db.Concerts.Add(concert);
    await db.SaveChangesAsync();

    var response = new ConcertResponse(
        concert.Id,
        concert.Title,
        artist.Name,
        concert.VenueName,
        concert.City,
        concert.Region,
        concert.Country,
        concert.ConcertDate,
        concert.CreatedAt,
        concert.UpdatedAt);

    return Results.Created($"/api/me/concerts/{concert.Id}", response);
})
    .WithName("CreateMeConcert")
    .WithTags("My Concerts");

app.MapPut("/api/me/concerts/{id:int}", async (
    int id,
    ConcertRequest request,
    ClaimsPrincipal principal,
    AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var validationErrors = ValidateConcertRequest(request);

    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var concert = await db.Concerts
        .Include(existingConcert => existingConcert.Artist)
        .SingleOrDefaultAsync(existingConcert =>
            existingConcert.Id == id && existingConcert.UserProfileId == userProfile.Id);

    // Non-owned ids return 404 to avoid revealing whether another user's concert exists.
    if (concert is null)
    {
        return Results.NotFound();
    }

    var artist = await FindOrCreateArtistAsync(request.ArtistName, db);

    concert.Title = request.Title.Trim();
    concert.VenueName = TrimToNull(request.VenueName);
    concert.City = TrimToNull(request.City);
    concert.Region = TrimToNull(request.Region);
    concert.Country = TrimToNull(request.Country);
    concert.ConcertDate = request.ConcertDate;
    concert.Artist = artist;

    await db.SaveChangesAsync();

    return Results.Ok(new ConcertResponse(
        concert.Id,
        concert.Title,
        artist.Name,
        concert.VenueName,
        concert.City,
        concert.Region,
        concert.Country,
        concert.ConcertDate,
        concert.CreatedAt,
        concert.UpdatedAt));
})
    .WithName("UpdateMeConcert")
    .WithTags("My Concerts");

app.MapDelete("/api/me/concerts/{id:int}", async (int id, ClaimsPrincipal principal, AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var concert = await db.Concerts
        .SingleOrDefaultAsync(existingConcert =>
            existingConcert.Id == id && existingConcert.UserProfileId == userProfile.Id);

    // Non-owned ids return 404 to avoid revealing whether another user's concert exists.
    if (concert is null)
    {
        return Results.NotFound();
    }

    db.Concerts.Remove(concert);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .WithName("DeleteMeConcert")
    .WithTags("My Concerts");

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

app.MapGet("/api/public/artists", async (AppDbContext db) =>
{
    var artists = await db.Artists
        .AsNoTracking()
        .Select(artist => new
        {
            artist.Id,
            artist.Name,
            ConcertCount = artist.Concerts.Count,
            ReviewCount = artist.Concerts.SelectMany(concert => concert.Reviews).Count(),
            Concerts = artist.Concerts
                .Select(concert => new
                {
                    concert.Title,
                    concert.VenueName,
                    concert.City,
                    concert.Region,
                    concert.ConcertDate
                })
                .ToList()
        })
        .ToListAsync();

    var artistDtos = artists
        .OrderBy(artist => artist.Name)
        .Select(artist => new
        {
            artist.Id,
            artist.Name,
            artist.ConcertCount,
            artist.ReviewCount,
            LatestConcert = artist.Concerts
                .OrderByDescending(concert => concert.ConcertDate)
                .FirstOrDefault()
        })
        .ToList();

    return Results.Ok(artistDtos);
})
    .WithName("PublicArtists")
    .WithTags("Public");

app.MapGet("/api/public/activity", async (AppDbContext db) =>
{
    var activity = await db.ActivityEvents
        .AsNoTracking()
        .Select(activityEvent => new
        {
            activityEvent.Id,
            activityEvent.EventType,
            activityEvent.Summary,
            activityEvent.CreatedAt,
            UserDisplayName = activityEvent.UserProfile == null
                ? null
                : activityEvent.UserProfile.DisplayName,
            ConcertTitle = activityEvent.Concert == null
                ? null
                : activityEvent.Concert.Title
        })
        .ToListAsync();

    var activityDtos = activity
        .OrderByDescending(activityEvent => activityEvent.CreatedAt)
        .Take(25)
        .ToList();

    return Results.Ok(activityDtos);
})
    .WithName("PublicActivity")
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

static string? GetOAuthSubject(ClaimsPrincipal principal)
{
    return principal.FindFirstValue(ClaimTypes.NameIdentifier);
}

static string GetDisplayName(ClaimsPrincipal principal)
{
    return principal.FindFirstValue(ClaimTypes.Name)
        ?? principal.FindFirstValue(ClaimTypes.Email)
        ?? "Setlist Social User";
}

static async Task<UserProfile?> GetCurrentUserProfileAsync(ClaimsPrincipal principal, AppDbContext db)
{
    if (principal.Identity?.IsAuthenticated != true)
    {
        return null;
    }

    var oauthSubject = GetOAuthSubject(principal);

    if (oauthSubject is null)
    {
        return null;
    }

    return await db.UserProfiles
        .SingleOrDefaultAsync(user => user.OAuthSubject == oauthSubject);
}

static async Task<Artist> FindOrCreateArtistAsync(string artistName, AppDbContext db)
{
    var normalizedName = artistName.Trim();
    var artist = await db.Artists
        .FirstOrDefaultAsync(existingArtist => existingArtist.Name == normalizedName);

    if (artist is not null)
    {
        return artist;
    }

    artist = new Artist { Name = normalizedName };
    db.Artists.Add(artist);
    return artist;
}

static Dictionary<string, string[]> ValidateConcertRequest(ConcertRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Title))
    {
        errors[nameof(request.Title)] = ["Title is required."];
    }
    else if (request.Title.Trim().Length > 240)
    {
        errors[nameof(request.Title)] = ["Title must be 240 characters or fewer."];
    }

    if (string.IsNullOrWhiteSpace(request.ArtistName))
    {
        errors[nameof(request.ArtistName)] = ["Artist name is required."];
    }
    else if (request.ArtistName.Trim().Length > 200)
    {
        errors[nameof(request.ArtistName)] = ["Artist name must be 200 characters or fewer."];
    }

    if (request.ConcertDate == default)
    {
        errors[nameof(request.ConcertDate)] = ["Concert date is required."];
    }

    AddMaxLengthError(errors, nameof(request.VenueName), request.VenueName, 200);
    AddMaxLengthError(errors, nameof(request.City), request.City, 120);
    AddMaxLengthError(errors, nameof(request.Region), request.Region, 120);
    AddMaxLengthError(errors, nameof(request.Country), request.Country, 120);

    return errors;
}

static void AddMaxLengthError(
    Dictionary<string, string[]> errors,
    string fieldName,
    string? value,
    int maxLength)
{
    if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
    {
        errors[fieldName] = [$"{fieldName} must be {maxLength} characters or fewer."];
    }
}

static string? TrimToNull(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record ConcertRequest(
    string Title,
    string ArtistName,
    string? VenueName,
    string? City,
    string? Region,
    string? Country,
    DateTimeOffset ConcertDate);

public sealed record ConcertResponse(
    int Id,
    string Title,
    string ArtistName,
    string? VenueName,
    string? City,
    string? Region,
    string? Country,
    DateTimeOffset ConcertDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
