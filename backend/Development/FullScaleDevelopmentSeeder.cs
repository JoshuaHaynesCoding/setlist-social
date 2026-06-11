using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SetlistSocial.Api.Data;
using SetlistSocial.Api.Models;

namespace SetlistSocial.Api.Development;

public static class FullScaleDevelopmentSeeder
{
    private const string SeedUserPrefix = "seed-user-";
    private const int RandomSeed = 20260611;
    private static readonly DateTimeOffset BaseTime = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ActivityStart = new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly TasteGroup[] TasteGroups =
    [
        new("hip-hop", "Hip-Hop", ["crate", "808", "cipher", "boom", "verse", "rap", "sample", "mixtape"]),
        new("r&b-soul", "R&B/Soul", ["sade", "velvet", "soul", "slowjam", "honey", "silk", "groove", "falsetto"]),
        new("classic-rock", "Classic Rock", ["vinyl", "amp", "reprise", "highway", "analog", "riff", "record", "stereo"]),
        new("hard-rock-metal", "Hard Rock/Metal", ["metal", "pit", "doom", "riffs", "anvil", "forge", "volume", "feedback"]),
        new("electronic", "Electronic", ["neon", "pulse", "synth", "rave", "circuit", "club", "laser", "tempo"]),
        new("indie-alternative", "Indie/Alternative", ["indie", "loft", "porch", "static", "zine", "basement", "shoegaze", "cassette"]),
        new("pop", "Pop", ["glitter", "chorus", "radio", "starlit", "daylight", "hook", "mirror", "spark"]),
        new("jazz", "Jazz", ["blue", "brass", "bebop", "quartet", "swing", "smoke", "cadence", "improv"])
    ];

    private static readonly string[] HandleSuffixes = ["ghost", "archive", "files", "report", "alex", "mei", "diary", "club"];

    private static readonly string[] VenueNames =
    [
        "The Green Room", "Riverside Hall", "Metro Annex", "Blue Door Theater", "Warehouse 17",
        "The Lantern Room", "Summit Pavilion", "Echo Yard", "Union Stage", "Crescent Ballroom",
        "Harbor House", "The Marquee", "West Loop Hall", "Station Room", "Grand Park Stage"
    ];

    private static readonly (string City, string Region)[] Cities =
    [
        ("Chicago", "IL"), ("Milwaukee", "WI"), ("Minneapolis", "MN"), ("Detroit", "MI"), ("Nashville", "TN"),
        ("Atlanta", "GA"), ("Austin", "TX"), ("Denver", "CO"), ("Seattle", "WA"), ("Portland", "OR"),
        ("Los Angeles", "CA"), ("New York", "NY"), ("Philadelphia", "PA"), ("New Orleans", "LA"), ("Kansas City", "MO")
    ];

    private static readonly string[] GenericTags =
    [
        "Festival", "Small Venue", "Arena", "Outdoor", "Late Show", "Local Favorite", "Great Opener", "Encore Worthy",
        "First Time", "Road Trip", "High Energy", "Acoustic", "Dance Night", "Front Row", "Tour Stop", "All Ages"
    ];

    private static readonly string[] ReviewBodies =
    [
        "The set felt focused, loud, and completely worth the trip.",
        "Great crowd energy and a closer that made the whole night land.",
        "The pacing was strong from the opener through the encore.",
        "A few rough transitions, but the best songs sounded huge live.",
        "The venue fit the sound perfectly and the crowd stayed locked in.",
        "This is exactly the kind of show I want to remember later."
    ];

    public static async Task<FullScaleSeedResult> SeedAsync(
        AppDbContext db,
        string contentRootPath,
        bool reset,
        CancellationToken cancellationToken)
    {
        var hasExistingFullSeed = await db.UserProfiles
            .AnyAsync(user => user.OAuthSubject.StartsWith(SeedUserPrefix), cancellationToken);
        var hasAnyExistingData =
            await db.UserProfiles.AnyAsync(cancellationToken)
            || await db.Artists.AnyAsync(cancellationToken)
            || await db.Concerts.AnyAsync(cancellationToken)
            || await db.Reviews.AnyAsync(cancellationToken)
            || await db.WishlistItems.AnyAsync(cancellationToken)
            || await db.Tags.AnyAsync(cancellationToken)
            || await db.ActivityEvents.AnyAsync(cancellationToken);

        if (hasExistingFullSeed && !reset)
        {
            return await BuildResultAsync("already-seeded", reset, "Full-scale seed users already exist. Use reset=true to rebuild the dataset.", db, cancellationToken);
        }

        if (hasAnyExistingData && !reset)
        {
            return await BuildResultAsync("existing-data-requires-reset", reset, "The database already contains local app data. Use reset=true to clear local development data and create the full-scale seed.", db, cancellationToken);
        }

        if (reset)
        {
            await ResetGeneratedDataAsync(db, cancellationToken);
        }

        var random = new Random(RandomSeed);
        var users = CreateUsers();
        var artists = CreateArtists(contentRootPath);
        var tags = CreateTags();

        var artistBuckets = artists
            .GroupBy(artist => artist.Group.Key)
            .ToDictionary(group => group.Key, group => group.ToList());
        var tagByName = tags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);

        var concerts = CreateConcerts(random, users, artistBuckets, tagByName);
        var reviews = CreateReviews(random, users, concerts);
        var wishlistItems = CreateWishlistItems(random, users, artistBuckets);
        var activityEvents = CreateActivityEvents(random, users, concerts, reviews, wishlistItems);

        db.Tags.AddRange(tags);
        db.UserProfiles.AddRange(users.Select(user => user.Profile));
        db.Artists.AddRange(artists.Select(artist => artist.Artist));
        db.Concerts.AddRange(concerts.Select(concert => concert.Concert));
        db.Reviews.AddRange(reviews);
        db.WishlistItems.AddRange(wishlistItems);
        db.ActivityEvents.AddRange(activityEvents);

        await db.SaveChangesAsync(cancellationToken);

        return await BuildResultAsync("seeded", reset, "Seed data is fake, deterministic, and generated locally without external API calls.", db, cancellationToken);
    }

    private static async Task ResetGeneratedDataAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await db.ActivityEvents.ExecuteDeleteAsync(cancellationToken);
        await db.Reviews.ExecuteDeleteAsync(cancellationToken);
        await db.WishlistItems.ExecuteDeleteAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ConcertTag", cancellationToken);
        await db.Concerts.ExecuteDeleteAsync(cancellationToken);
        await db.Tags.ExecuteDeleteAsync(cancellationToken);
        await db.Artists.ExecuteDeleteAsync(cancellationToken);
        await db.UserProfiles
            .Where(user => user.OAuthSubject.StartsWith(SeedUserPrefix))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static List<SeedUser> CreateUsers()
    {
        var users = new List<SeedUser>(512);

        for (var i = 0; i < 512; i++)
        {
            var group = TasteGroups[i % TasteGroups.Length];
            var displayName = CreateHandle(group, i / TasteGroups.Length);

            users.Add(new SeedUser(
                new UserProfile
                {
                    DisplayName = displayName,
                    OAuthSubject = $"{SeedUserPrefix}{i + 1:0000}",
                    Bio = $"Simulated {group.Label} listener for local development seed data.",
                    CreatedAt = BaseTime.AddDays(-(i % 180)),
                    UpdatedAt = BaseTime.AddDays(-(i % 30))
                },
                group));
        }

        return users;
    }

    private static List<SeedArtist> CreateArtists(string contentRootPath)
    {
        var curatedArtists = LoadCuratedArtistNames(contentRootPath);
        var artists = new List<SeedArtist>();

        foreach (var group in TasteGroups)
        {
            if (!curatedArtists.TryGetValue(group.Key, out var artistNames) || artistNames.Count == 0)
            {
                throw new InvalidOperationException($"The curated seed artist list does not include any artists for '{group.Key}'.");
            }

            for (var i = 0; i < artistNames.Count; i++)
            {
                artists.Add(new SeedArtist(
                    new Artist
                    {
                        Name = artistNames[i],
                        CreatedAt = BaseTime.AddDays(-200 + i),
                        UpdatedAt = BaseTime.AddDays(-20 + i % 20)
                    },
                    group));
            }
        }

        return artists;
    }

    private static string CreateHandle(TasteGroup group, int groupUserIndex)
    {
        var root = group.HandleWords[groupUserIndex % group.HandleWords.Length];
        var suffix = HandleSuffixes[(groupUserIndex / group.HandleWords.Length) % HandleSuffixes.Length];
        return $"@{root}{suffix}";
    }

    private static Dictionary<string, List<string>> LoadCuratedArtistNames(string contentRootPath)
    {
        var listPath = ResolveArtistListPath(contentRootPath);
        var rawText = File.ReadAllText(listPath);
        var lines = NormalizeArtistListText(rawText)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var validKeys = TasteGroups.Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var artistsByGroup = TasteGroups.ToDictionary(
            group => group.Key,
            _ => new List<string>(),
            StringComparer.OrdinalIgnoreCase);
        string? currentGroup = null;

        foreach (var rawLine in lines)
        {
            var line = CleanArtistListLine(rawLine);

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.EndsWith(':'))
            {
                var possibleGroup = line.TrimEnd(':').Trim().ToLowerInvariant();
                currentGroup = validKeys.Contains(possibleGroup) ? possibleGroup : null;
                continue;
            }

            if (currentGroup is null)
            {
                continue;
            }

            var artistName = line.TrimStart('-', '*').Trim();

            if (!string.IsNullOrWhiteSpace(artistName)
                && !artistsByGroup[currentGroup].Contains(artistName, StringComparer.OrdinalIgnoreCase))
            {
                artistsByGroup[currentGroup].Add(artistName);
            }
        }

        return artistsByGroup;
    }

    private static string ResolveArtistListPath(string contentRootPath)
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "docs", "SEED_ARTIST_LISTS.txt")),
            Path.GetFullPath(Path.Combine(contentRootPath, "docs", "SEED_ARTIST_LISTS.txt")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "SEED_ARTIST_LISTS.txt"))
        };

        return candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Could not find docs/SEED_ARTIST_LISTS.txt for the development seed process.");
    }

    private static string NormalizeArtistListText(string text)
    {
        return Regex.Replace(text, @"\\'([0-9a-fA-F]{2})", match =>
        {
            var value = Convert.ToInt32(match.Groups[1].Value, 16);
            return char.ConvertFromUtf32(value);
        });
    }

    private static string CleanArtistListLine(string line)
    {
        line = line.Trim().TrimEnd('\\').Trim();
        line = Regex.Replace(line, @"\\[a-zA-Z]+\d* ?", string.Empty).Trim();
        line = Regex.Replace(line, @"^[a-zA-Z]+\d+\s*", string.Empty).Trim();
        return line.Trim('{', '}').Trim();
    }

    private static List<Tag> CreateTags()
    {
        var tags = TasteGroups
            .Select(group => new Tag
            {
                Name = group.Label,
                CreatedAt = BaseTime,
                UpdatedAt = BaseTime
            })
            .ToList();

        tags.AddRange(GenericTags.Select(tag => new Tag
        {
            Name = tag,
            CreatedAt = BaseTime,
            UpdatedAt = BaseTime
        }));

        return tags;
    }

    private static List<SeedConcert> CreateConcerts(
        Random random,
        IReadOnlyList<SeedUser> users,
        IReadOnlyDictionary<string, List<SeedArtist>> artistBuckets,
        IReadOnlyDictionary<string, Tag> tagByName)
    {
        var concerts = new List<SeedConcert>(3200);

        for (var i = 0; i < 3200; i++)
        {
            var user = users[random.Next(users.Count)];
            var artist = ChooseArtist(random, user.Group, artistBuckets);
            var venue = VenueNames[random.Next(VenueNames.Length)];
            var city = Cities[random.Next(Cities.Length)];
            var concertDate = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow.AddDays(180));
            var tagNames = new[]
            {
                artist.Group.Label,
                GenericTags[random.Next(GenericTags.Length)],
                GenericTags[random.Next(GenericTags.Length)]
            };

            var concert = new Concert
            {
                Title = $"{artist.Artist.Name} at {venue}",
                VenueName = venue,
                City = city.City,
                Region = city.Region,
                Country = "USA",
                ConcertDate = concertDate,
                UserProfile = user.Profile,
                Artist = artist.Artist,
                CreatedAt = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow),
                UpdatedAt = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow)
            };

            foreach (var tagName in tagNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                concert.Tags.Add(tagByName[tagName]);
            }

            concerts.Add(new SeedConcert(concert, user, artist));
        }

        return concerts;
    }

    private static List<Review> CreateReviews(Random random, IReadOnlyList<SeedUser> users, IReadOnlyList<SeedConcert> concerts)
    {
        var reviews = new List<Review>(3600);

        for (var i = 0; i < 3600; i++)
        {
            var concert = concerts[random.Next(concerts.Count)];
            var reviewer = random.NextDouble() < 0.75
                ? concert.User
                : ChooseUser(random, users, concert.Artist.Group);

            reviews.Add(new Review
            {
                Rating = random.Next(3, 6),
                Body = ReviewBodies[random.Next(ReviewBodies.Length)],
                UserProfile = reviewer.Profile,
                Concert = concert.Concert,
                CreatedAt = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow),
                UpdatedAt = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow)
            });
        }

        return reviews;
    }

    private static List<WishlistItem> CreateWishlistItems(
        Random random,
        IReadOnlyList<SeedUser> users,
        IReadOnlyDictionary<string, List<SeedArtist>> artistBuckets)
    {
        var wishlistItems = new List<WishlistItem>(2000);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (wishlistItems.Count < 2000)
        {
            var userIndex = random.Next(users.Count);
            var user = users[userIndex];
            var artist = ChooseArtist(random, user.Group, artistBuckets);
            var key = $"{userIndex}:{artist.Artist.Name}";

            if (!seen.Add(key))
            {
                continue;
            }

            wishlistItems.Add(new WishlistItem
            {
                Notes = $"Simulated wishlist pick for a {user.Group.Label} listener.",
                SourceName = "Simulated seed",
                UserProfile = user.Profile,
                Artist = artist.Artist,
                CreatedAt = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow),
                UpdatedAt = RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow)
            });
        }

        return wishlistItems;
    }

    private static List<ActivityEvent> CreateActivityEvents(
        Random random,
        IReadOnlyList<SeedUser> users,
        IReadOnlyList<SeedConcert> concerts,
        IReadOnlyList<Review> reviews,
        IReadOnlyList<WishlistItem> wishlistItems)
    {
        var activityEvents = new List<ActivityEvent>(10500);

        for (var i = 0; i < users.Count; i++)
        {
            activityEvents.Add(new ActivityEvent
            {
                EventType = "New user",
                Summary = $"{users[i].Profile.DisplayName} joined Setlist Social.",
                UserProfile = users[i].Profile,
                CreatedAt = RandomActivityDate(random)
            });
        }

        while (activityEvents.Count < 10500)
        {
            var eventType = random.Next(4);
            var createdAt = RandomActivityDate(random);

            switch (eventType)
            {
                case 0:
                    var concert = concerts[random.Next(concerts.Count)];
                    activityEvents.Add(new ActivityEvent
                    {
                        EventType = "Added concert",
                        Summary = $"{concert.User.Profile.DisplayName} added {concert.Concert.Title}.",
                        UserProfile = concert.User.Profile,
                        Concert = concert.Concert,
                        CreatedAt = createdAt
                    });
                    break;
                case 1:
                    var review = reviews[random.Next(reviews.Count)];
                    activityEvents.Add(new ActivityEvent
                    {
                        EventType = "Posted review",
                        Summary = $"{review.UserProfile.DisplayName} posted a review for {review.Concert.Artist.Name}.",
                        UserProfile = review.UserProfile,
                        Concert = review.Concert,
                        CreatedAt = createdAt
                    });
                    break;
                case 2:
                    var wishlistItem = wishlistItems[random.Next(wishlistItems.Count)];
                    activityEvents.Add(new ActivityEvent
                    {
                        EventType = "Saved artist",
                        Summary = $"{wishlistItem.UserProfile.DisplayName} saved {wishlistItem.Artist?.Name} to a wishlist.",
                        UserProfile = wishlistItem.UserProfile,
                        CreatedAt = createdAt
                    });
                    break;
                default:
                    var user = users[random.Next(users.Count)];
                    activityEvents.Add(new ActivityEvent
                    {
                        EventType = "Explored artists",
                        Summary = $"{user.Profile.DisplayName} explored more {user.Group.Label} artists.",
                        UserProfile = user.Profile,
                        CreatedAt = createdAt
                    });
                    break;
            }
        }

        return activityEvents;
    }

    private static SeedArtist ChooseArtist(Random random, TasteGroup preferredGroup, IReadOnlyDictionary<string, List<SeedArtist>> artistBuckets)
    {
        var group = random.NextDouble() < 0.8
            ? preferredGroup
            : TasteGroups[random.Next(TasteGroups.Length)];

        var artists = artistBuckets[group.Key];
        return artists[random.Next(artists.Count)];
    }

    private static SeedUser ChooseUser(Random random, IReadOnlyList<SeedUser> users, TasteGroup preferredGroup)
    {
        var matchingUsers = users.Where(user => user.Group.Key == preferredGroup.Key).ToList();
        return matchingUsers[random.Next(matchingUsers.Count)];
    }

    private static DateTimeOffset RandomActivityDate(Random random)
    {
        return RandomDateBetween(random, ActivityStart, DateTimeOffset.UtcNow, weightRecent: true);
    }

    private static DateTimeOffset RandomDateBetween(
        Random random,
        DateTimeOffset start,
        DateTimeOffset end,
        bool weightRecent = false)
    {
        var range = end - start;
        var fraction = weightRecent
            ? Math.Pow(random.NextDouble(), 0.55)
            : random.NextDouble();

        return start.AddSeconds(range.TotalSeconds * fraction);
    }

    private static async Task<FullScaleSeedResult> BuildResultAsync(
        string status,
        bool reset,
        string note,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        return new FullScaleSeedResult(
            status,
            reset,
            note,
            await db.UserProfiles.CountAsync(cancellationToken),
            await db.Artists.CountAsync(cancellationToken),
            await db.Concerts.CountAsync(cancellationToken),
            await db.Reviews.CountAsync(cancellationToken),
            await db.WishlistItems.CountAsync(cancellationToken),
            await db.Tags.CountAsync(cancellationToken),
            await db.ActivityEvents.CountAsync(cancellationToken));
    }

    private sealed record TasteGroup(string Key, string Label, string[] HandleWords);
    private sealed record SeedUser(UserProfile Profile, TasteGroup Group);
    private sealed record SeedArtist(Artist Artist, TasteGroup Group);
    private sealed record SeedConcert(Concert Concert, SeedUser User, SeedArtist Artist);
}

public sealed record FullScaleSeedResult(
    string Status,
    bool Reset,
    string Note,
    int Users,
    int Artists,
    int Concerts,
    int Reviews,
    int WishlistItems,
    int Tags,
    int ActivityEvents);
