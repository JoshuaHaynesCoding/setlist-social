# Setlist Social Backend

ASP.NET Core Minimal API targeting .NET 10.

## Current Features

- Swagger/OpenAPI
- Public health endpoint: `GET /api/health`
- EF Core `AppDbContext`
- SQLite provider for local development
- PostgreSQL provider installed for planned production database support
- Initial domain models and relationships
- Public stats endpoint: `GET /api/public/stats`
- Development-only seed endpoint: `POST /api/dev/seed`
- Development-only full-scale seed/reset endpoint: `POST /api/dev/seed/full?reset=true`
- Google OAuth/OIDC foundation with cookie authentication
- Current user endpoint: `GET /api/me`
- Public Last.fm artist search endpoint: `GET /api/external/lastfm/search?artist=ARTIST_NAME`
- SignalR public activity hub: `/hubs/activity`

## Local Database

Local development uses SQLite through `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=setlist-social-dev.db"
  }
}
```

The `.db` file is ignored by Git and should not be committed.

## Authentication Configuration

Set these values through environment variables or local user secrets. Do not commit real values.

- `Google__ClientId`
- `Google__ClientSecret`
- `FrontendUrl`

Local login starts at:

```bash
open http://localhost:5050/api/auth/login
```

Current signed-in user:

```bash
curl -i http://localhost:5050/api/me
```

## E2E Test Auth

Playwright tests can start the backend with `E2E__EnableTestAuth=true` in the `Development` environment. That flag maps `POST /api/dev/auth/test-login` for automated browser tests only.

Do not enable `E2E__EnableTestAuth` for normal local demos or production. Google OAuth remains the real application login path.

## Last.fm Configuration

Set the Last.fm API key with user-secrets for local development. Do not commit real values.

```bash
dotnet user-secrets set "LastFm:ApiKey" "YOUR_LASTFM_API_KEY"
```

Artist search:

```bash
curl "http://localhost:5050/api/external/lastfm/search?artist=cher"
```

## Migration Commands

Run from the `backend/` folder:

```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If `dotnet ef` is not installed globally:

```bash
dotnet tool install --global dotnet-ef
```

## Run Locally

```bash
dotnet run
```

Health check:

```bash
curl http://localhost:5050/api/health
```

Public stats:

```bash
curl http://localhost:5050/api/public/stats
```

Seed local development data:

```bash
curl -X POST http://localhost:5050/api/dev/seed
```

The seed endpoint is only mapped in the `Development` environment. It creates a small sample dataset with 3 users, 5 artists, 5 concerts, 5 reviews, 3 wishlist items, several activity events, and several tags.

If any app data already exists, the endpoint returns `already-seeded` and does not create duplicates. Check the current database counts with:

```bash
curl http://localhost:5050/api/public/stats
```

Seed full-scale simulated local development data into an empty local database:

```bash
curl -X POST "http://localhost:5050/api/dev/seed/full"
```

If local app data already exists, reset and rebuild the full-scale simulated dataset:

```bash
curl -X POST "http://localhost:5050/api/dev/seed/full?reset=true"
```

The full-scale seed endpoint is only mapped in the `Development` environment. It creates deterministic simulated data locally, including at least 500 generated user profiles, more than 5,000 domain records across artists/concerts/reviews/wishlist/tags, and more than 10,000 activity events.

Artist records come from the curated local list at `docs/SEED_ARTIST_LISTS.txt`. The seed process reads that file by genre and does not call Last.fm.

The generated users are spread across simulated music taste groups:

- hip-hop
- R&B/soul
- classic rock
- hard rock/metal
- electronic
- indie/alternative
- pop
- jazz

The seed process does not require external API access. The reset option clears local app/domain data and generated seed users from the development database, while preserving non-generated `UserProfile` rows where possible. Treat reset as a local development operation only.

## Not Implemented Yet

- Production PostgreSQL configuration
- Full protected CRUD beyond the current My Concerts and Wishlist foundations
