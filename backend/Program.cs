using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SetlistSocial.Api.Data;
using SetlistSocial.Api.Development;
using SetlistSocial.Api.External;
using SetlistSocial.Api.Hubs;
using SetlistSocial.Api.Models;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    DatabaseConfiguration.Configure(options, builder.Configuration, builder.Environment);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = GetAllowedFrontendOrigins(builder.Configuration, builder.Environment);

        if (allowedOrigins.Count > 0)
        {
            policy.WithOrigins([.. allowedOrigins])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
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
        options.Cookie.SameSite = builder.Environment.IsProduction()
            ? SameSiteMode.None
            : SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

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
builder.Services.AddHttpClient<ILastFmClient, LastFmClient>();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await ApplyMigrationsOnStartupAsync(app);
await ApplySeedOnStartupAsync(app);

app.UseForwardedHeaders();

if (GetAllowedFrontendOrigins(app.Configuration, app.Environment).Count > 0)
{
    app.UseCors(FrontendCorsPolicy);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithTags("Health");

app.MapHub<ActivityHub>("/hubs/activity");

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

if ((app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    && app.Configuration.GetValue<bool>("E2E:EnableTestAuth"))
{
    app.MapPost("/api/dev/auth/test-login", async (
        HttpContext context,
        AppDbContext db,
        string? subject,
        string? displayName,
        bool? reset) =>
    {
        var oauthSubject = string.IsNullOrWhiteSpace(subject)
            ? "e2e-test-user"
            : $"e2e-{subject.Trim()}";
        var safeDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? "@e2etester"
            : displayName.Trim();

        var userProfile = await db.UserProfiles
            .SingleOrDefaultAsync(user => user.OAuthSubject == oauthSubject);

        if (userProfile is null)
        {
            userProfile = new UserProfile
            {
                DisplayName = safeDisplayName,
                OAuthSubject = oauthSubject
            };

            db.UserProfiles.Add(userProfile);
            await db.SaveChangesAsync();
        }
        else
        {
            userProfile.DisplayName = safeDisplayName;
            await db.SaveChangesAsync();
        }

        if (reset == true)
        {
            await ResetE2EUserDataAsync(db, userProfile.Id);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, oauthSubject),
            new Claim(ClaimTypes.Name, safeDisplayName)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Results.Ok(new { status = "signed-in", displayName = safeDisplayName });
    })
        .WithName("E2ETestLogin")
        .WithTags("Development");
}

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

app.MapPost("/api/me/concerts", async (
    ConcertRequest request,
    ClaimsPrincipal principal,
    AppDbContext db,
    IHubContext<ActivityHub> activityHub) =>
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
    var concertDisplayText = FormatConcertDisplayText(artist.Name, concert.VenueName);
    var activityEvent = new ActivityEvent
    {
        EventType = "Added concert",
        Summary = $"{userProfile.DisplayName} added {concertDisplayText}.",
        UserProfileId = userProfile.Id,
        Concert = concert,
        CreatedAt = DateTimeOffset.UtcNow
    };
    db.ActivityEvents.Add(activityEvent);
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

    await BroadcastActivityAsync(
        activityHub,
        new PublicActivityEventResponse(
            activityEvent.EventType,
            activityEvent.Summary,
            userProfile.DisplayName,
            activityEvent.CreatedAt,
            artist.Name,
            concertDisplayText));

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

app.MapGet("/api/me/wishlist", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var wishlistItems = await db.WishlistItems
        .AsNoTracking()
        .Where(item => item.UserProfileId == userProfile.Id)
        .Select(item => new WishlistItemResponse(
            item.Id,
            item.Artist == null ? "Unknown artist" : item.Artist.Name,
            item.SourceUrl,
            item.SourceName,
            item.CreatedAt,
            item.UpdatedAt))
        .ToListAsync();

    return Results.Ok(wishlistItems
        .OrderByDescending(item => item.CreatedAt)
        .ToList());
})
    .WithName("MeWishlist")
    .WithTags("My Wishlist");

app.MapPost("/api/me/wishlist", async (
    WishlistItemRequest request,
    ClaimsPrincipal principal,
    AppDbContext db,
    IHubContext<ActivityHub> activityHub) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var validationErrors = ValidateWishlistItemRequest(request);

    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var normalizedArtistName = request.ArtistName.Trim();
    var normalizedArtistNameUpper = normalizedArtistName.ToUpperInvariant();

    var existingWishlistItem = await db.WishlistItems
        .AsNoTracking()
        .Where(item => item.UserProfileId == userProfile.Id && item.Artist != null)
        .Select(item => new
        {
            item.Id,
            ArtistName = item.Artist!.Name,
            item.SourceUrl,
            item.SourceName,
            item.CreatedAt,
            item.UpdatedAt
        })
        .FirstOrDefaultAsync(item => item.ArtistName.ToUpper() == normalizedArtistNameUpper);

    if (existingWishlistItem is not null)
    {
        return Results.Conflict(new
        {
            message = "Artist is already in your wishlist.",
            item = new WishlistItemResponse(
                existingWishlistItem.Id,
                existingWishlistItem.ArtistName,
                existingWishlistItem.SourceUrl,
                existingWishlistItem.SourceName,
                existingWishlistItem.CreatedAt,
                existingWishlistItem.UpdatedAt)
        });
    }

    var artist = await FindOrCreateArtistAsync(normalizedArtistName, db);
    var wishlistItem = new WishlistItem
    {
        Artist = artist,
        UserProfileId = userProfile.Id,
        SourceName = TrimToNull(request.SourceName),
        SourceUrl = TrimToNull(request.SourceUrl)
    };

    db.WishlistItems.Add(wishlistItem);
    var activityEvent = new ActivityEvent
    {
        EventType = "Saved artist",
        Summary = $"{userProfile.DisplayName} saved {artist.Name} to their wishlist.",
        UserProfileId = userProfile.Id,
        CreatedAt = DateTimeOffset.UtcNow
    };
    db.ActivityEvents.Add(activityEvent);
    await db.SaveChangesAsync();

    var response = new WishlistItemResponse(
        wishlistItem.Id,
        artist.Name,
        wishlistItem.SourceUrl,
        wishlistItem.SourceName,
        wishlistItem.CreatedAt,
        wishlistItem.UpdatedAt);

    await BroadcastActivityAsync(
        activityHub,
        new PublicActivityEventResponse(
            activityEvent.EventType,
            activityEvent.Summary,
            userProfile.DisplayName,
            activityEvent.CreatedAt,
            artist.Name,
            null));

    return Results.Created($"/api/me/wishlist/{wishlistItem.Id}", response);
})
    .WithName("CreateMeWishlistItem")
    .WithTags("My Wishlist");

app.MapDelete("/api/me/wishlist/{id:int}", async (int id, ClaimsPrincipal principal, AppDbContext db) =>
{
    var userProfile = await GetCurrentUserProfileAsync(principal, db);

    if (userProfile is null)
    {
        return Results.Unauthorized();
    }

    var wishlistItem = await db.WishlistItems
        .SingleOrDefaultAsync(item => item.Id == id && item.UserProfileId == userProfile.Id);

    // Non-owned ids return 404 to avoid revealing whether another user's wishlist item exists.
    if (wishlistItem is null)
    {
        return Results.NotFound();
    }

    db.WishlistItems.Remove(wishlistItem);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .WithName("DeleteMeWishlistItem")
    .WithTags("My Wishlist");

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

app.MapGet("/api/external/lastfm/search", async (
    string? artist,
    ILastFmClient lastFmClient,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(artist))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(artist)] = ["Artist query is required."]
        });
    }

    if (string.IsNullOrWhiteSpace(configuration["LastFm:ApiKey"]))
    {
        return Results.Problem(
            title: "Last.fm is not configured.",
            detail: "Set LastFm__ApiKey before using artist discovery.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var results = await lastFmClient.SearchArtistsAsync(artist, cancellationToken);
    return Results.Ok(results);
})
    .WithName("LastFmArtistSearch")
    .WithTags("External");

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
                : activityEvent.Concert.Title,
            ArtistName = activityEvent.Concert == null
                ? null
                : activityEvent.Concert.Artist.Name
        })
        .ToListAsync();

    var activityDtos = activity
        .OrderByDescending(activityEvent => activityEvent.CreatedAt)
        .Take(25)
        .Select(activityEvent => new
        {
            activityEvent.Id,
            Type = activityEvent.EventType,
            Message = activityEvent.Summary,
            activityEvent.UserDisplayName,
            activityEvent.CreatedAt,
            ArtistDisplayText = activityEvent.ArtistName,
            ConcertDisplayText = activityEvent.ConcertTitle,
            activityEvent.EventType,
            activityEvent.Summary,
            activityEvent.ConcertTitle
        })
        .ToList();

    return Results.Ok(activityDtos);
})
    .WithName("PublicActivity")
    .WithTags("Public");

if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/dev/seed/full", async (bool? reset, AppDbContext db, CancellationToken cancellationToken) =>
    {
        var result = await FullScaleDevelopmentSeeder.SeedAsync(db, app.Environment.ContentRootPath, reset == true, cancellationToken);
        return Results.Ok(result);
    })
        .WithName("SeedFullScaleDevelopmentData")
        .WithTags("Development");

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

var renderPort = Environment.GetEnvironmentVariable("PORT");

if (int.TryParse(renderPort, out var port))
{
    app.Run($"http://0.0.0.0:{port}");
}
else
{
    app.Run();
}

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

static Dictionary<string, string[]> ValidateWishlistItemRequest(WishlistItemRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.ArtistName))
    {
        errors[nameof(request.ArtistName)] = ["Artist name is required."];
    }
    else if (request.ArtistName.Trim().Length > 200)
    {
        errors[nameof(request.ArtistName)] = ["Artist name must be 200 characters or fewer."];
    }

    AddMaxLengthError(errors, nameof(request.SourceName), request.SourceName, 120);
    AddMaxLengthError(errors, nameof(request.SourceUrl), request.SourceUrl, 1000);

    if (!string.IsNullOrWhiteSpace(request.SourceUrl)
        && !Uri.TryCreate(request.SourceUrl.Trim(), UriKind.Absolute, out _))
    {
        errors[nameof(request.SourceUrl)] = ["Source URL must be an absolute URL."];
    }

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

static async Task ResetE2EUserDataAsync(AppDbContext db, int userProfileId)
{
    var concerts = await db.Concerts
        .Include(concert => concert.Tags)
        .Where(concert => concert.UserProfileId == userProfileId)
        .ToListAsync();

    await db.ActivityEvents
        .Where(activityEvent => activityEvent.UserProfileId == userProfileId)
        .ExecuteDeleteAsync();
    await db.Reviews
        .Where(review => review.UserProfileId == userProfileId)
        .ExecuteDeleteAsync();
    await db.WishlistItems
        .Where(item => item.UserProfileId == userProfileId)
        .ExecuteDeleteAsync();

    db.Concerts.RemoveRange(concerts);
    await db.SaveChangesAsync();
}

static string FormatConcertDisplayText(string artistName, string? venueName)
{
    return string.IsNullOrWhiteSpace(venueName)
        ? artistName
        : $"{artistName} at {venueName.Trim()}";
}

static Task BroadcastActivityAsync(IHubContext<ActivityHub> activityHub, PublicActivityEventResponse activity)
{
    return activityHub.Clients.All.SendAsync("activityCreated", activity);
}

static async Task ApplyMigrationsOnStartupAsync(WebApplication app)
{
    if (!app.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup"))
    {
        return;
    }

    var logger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigrations");

    logger.LogInformation("Applying EF Core database migrations on startup.");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    logger.LogInformation("EF Core database migrations completed.");
}

static async Task ApplySeedOnStartupAsync(WebApplication app)
{
    if (!app.Configuration.GetValue<bool>("Seed:RunOnStartup"))
    {
        return;
    }

    var logger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("ProductionSeed");

    logger.LogInformation("Production startup seed requested. Checking database state.");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var result = await FullScaleDevelopmentSeeder.SeedProductionAsync(
        db,
        app.Environment.ContentRootPath,
        CancellationToken.None);

    if (result.Status is "already-seeded" or "existing-data-skip")
    {
        logger.LogInformation(
            "Production startup seed skipped with status {Status}. Users: {Users}, domain records: {DomainRecords}, activity events: {ActivityEvents}.",
            result.Status,
            result.Users,
            result.Artists + result.Concerts + result.Reviews + result.WishlistItems + result.Tags,
            result.ActivityEvents);
        return;
    }

    logger.LogInformation(
        "Production startup seed finished with status {Status}. Users: {Users}, domain records: {DomainRecords}, activity events: {ActivityEvents}.",
        result.Status,
        result.Users,
        result.Artists + result.Concerts + result.Reviews + result.WishlistItems + result.Tags,
        result.ActivityEvents);
}

static List<string> GetAllowedFrontendOrigins(IConfiguration configuration, IHostEnvironment environment)
{
    var origins = new List<string>();

    if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
    {
        origins.Add("http://localhost:5173");
        origins.Add("http://127.0.0.1:5173");
    }

    origins.Add("https://setlist-social.vercel.app");

    var frontendUrl = configuration["FrontendUrl"];

    if (Uri.TryCreate(frontendUrl, UriKind.Absolute, out var uri))
    {
        origins.Add(uri.GetLeftPart(UriPartial.Authority));
    }

    return origins.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
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

public sealed record WishlistItemRequest(
    string ArtistName,
    string? SourceUrl,
    string? SourceName);

public sealed record WishlistItemResponse(
    int Id,
    string ArtistName,
    string? SourceUrl,
    string? SourceName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PublicActivityEventResponse(
    string Type,
    string Message,
    string? UserDisplayName,
    DateTimeOffset CreatedAt,
    string? ArtistDisplayText,
    string? ConcertDisplayText);

public partial class Program;
