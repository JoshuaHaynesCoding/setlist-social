# Setlist Social Backend

ASP.NET Core Minimal API targeting .NET 10.

## Current Features

- Swagger/OpenAPI
- Public health endpoint: `GET /api/health`
- EF Core `AppDbContext`
- SQLite provider for local development
- PostgreSQL provider support for production
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

## Production Database

Production should use PostgreSQL through the `ConnectionStrings__DefaultConnection` environment variable. Local development continues to use SQLite unless a PostgreSQL-style connection string or explicit provider is configured.

Required production backend environment variables:

```text
ConnectionStrings__DefaultConnection
Database__RunMigrationsOnStartup
Seed__RunOnStartup
Google__ClientId
Google__ClientSecret
FrontendUrl
LastFm__ApiKey
```

Optional explicit provider setting:

```text
Database__Provider=PostgreSQL
```

For Render, set `ConnectionStrings__DefaultConnection` to a PostgreSQL connection string in Npgsql format, for example using keys such as `Host`, `Database`, `Username`, `Password`, and `SSL Mode`. Do not commit the real value.

The app reads Render's `PORT` environment variable automatically. Locally, `dotnet run` still uses the normal ASP.NET Core launch settings.

If `Database__RunMigrationsOnStartup=true`, the backend applies EF Core migrations during startup. This is disabled by default. It does not call `EnsureCreated`, does not reset data, and does not run development seed endpoints.

If `Seed__RunOnStartup=true`, the backend runs a production-safe simulated seed after the migration startup step. This is disabled by default. It does not reset, delete, truncate, or drop data. It skips if generated seed users already exist, and it skips if domain data already exists so rows are not duplicated. Existing real OAuth users can remain in the database while simulated seed users are added.

## Local Authentication Configuration

Set these values as environment variables when starting the backend. Do not commit real values.

- `Google__ClientId`
- `Google__ClientSecret`
- `FrontendUrl`
- `LastFm__ApiKey`

Local startup example:

```bash
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5050 \
Google__ClientId="YOUR_GOOGLE_CLIENT_ID" \
Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET" \
FrontendUrl="http://localhost:5173" \
LastFm__ApiKey="YOUR_LASTFM_API_KEY" \
dotnet run
```

The double underscore maps to nested ASP.NET Core configuration keys. For example, `Google__ClientId` is read by the app as `Google:ClientId`.

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

Set `LastFm__ApiKey` in the same backend startup command. Do not commit the real value.

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

Production migration option:

```bash
ASPNETCORE_ENVIRONMENT=Production \
ConnectionStrings__DefaultConnection="YOUR_POSTGRES_CONNECTION_STRING" \
dotnet ef database update --project backend/SetlistSocial.Api.csproj --startup-project backend/SetlistSocial.Api.csproj
```

Use a real production connection string only in your shell or hosting environment. Do not place it in source control.

Render deployment option:

1. Create or attach a PostgreSQL database.
2. Create a Render web service for the backend using `backend/Dockerfile`.
3. Set the backend environment variables listed above.
4. Run EF Core migrations against the PostgreSQL database as a manual release step, or set `Database__RunMigrationsOnStartup=true` so the service applies migrations when it starts.
5. To populate an empty production database for the class project demo, temporarily set `Seed__RunOnStartup=true` and redeploy/restart after migrations are enabled.
6. Confirm `GET /api/health` returns `{ "status": "ok" }`.
7. Confirm `GET /api/public/stats` shows seeded counts, then set `Seed__RunOnStartup=false` or remove the variable and redeploy/restart.

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

- Full protected CRUD beyond the current My Concerts and Wishlist foundations
