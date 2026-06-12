# Setlist Social

[![CI](https://github.com/JoshuaHaynesCoding/setlist-social/actions/workflows/ci.yml/badge.svg)](https://github.com/JoshuaHaynesCoding/setlist-social/actions/workflows/ci.yml)

Setlist Social is a music and events app where fans discover artists, save wishlist artists, track their own concerts, and watch public community activity update live.

## Deployed Application

- Frontend: `https://setlist-social.vercel.app`
- Backend API: `https://setlist-social.onrender.com`
- Backend health check: `https://setlist-social.onrender.com/api/health`

## Demo Account Instructions

Setlist Social uses Google OAuth/OIDC. For evaluation, open the deployed frontend and choose **Sign in with Google**. If the Google OAuth app is still in testing mode, the evaluator's Google email must be added as an allowed test user in the Google Cloud OAuth consent screen. There is no shared password-based demo account and no OAuth credentials are committed to this repository.

## Current Features

- React + Vite + JavaScript frontend with React Router.
- Public routes for landing, about, stats, artists, activity, and discovery.
- Protected routes for dashboard, profile, my concerts, wishlist, and settings.
- Google OAuth/OIDC login, callback, logout, and `/api/me` session check.
- User-owned concert CRUD with backend-enforced user isolation.
- User-owned wishlist flow connected to Last.fm discovery.
- Last.fm artist search through the backend so the API key is not exposed to React.
- EF Core persistence with SQLite for local development and PostgreSQL for production.
- SignalR public activity feed for live community updates.
- Full-scale deterministic seed process with 500+ profiles, 5,000+ domain records, and 10,000+ activity/interactions.
- Backend integration tests, frontend Vitest tests, Playwright E2E tests, and GitHub Actions CI.

## Repository Structure

- `frontend/` - React + Vite frontend, routes, reusable components, Vitest tests, and Playwright E2E tests.
- `backend/` - ASP.NET Core Minimal API on .NET 10, EF Core, OAuth, Last.fm client, SignalR hub, seed tooling, and Dockerfile.
- `backend.tests/` - xUnit integration tests with a test auth scheme.
- `docs/` - design, architecture, prompt log, AI reflection, security review, and accessibility report.

## Prerequisites

- Node.js 24 or compatible current Node version.
- .NET 10 SDK.
- EF Core CLI: `dotnet tool install --global dotnet-ef --version 10.0.9`
- SQLite for local development through EF Core.
- PostgreSQL only if running the production-style provider locally.
- Google OAuth client configured with local and deployed redirect URIs.
- Last.fm API key.

## Required Environment Variables

Backend:

| Variable | Description |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` locally, `Production` on Render. |
| `ASPNETCORE_URLS` | Local backend URL, usually `http://localhost:5050`. |
| `ConnectionStrings__DefaultConnection` | Database connection string. Local SQLite comes from `appsettings.Development.json`; production uses PostgreSQL. |
| `Database__Provider` | Optional explicit provider, usually `PostgreSQL` in CI/production-style runs. |
| `Database__RunMigrationsOnStartup` | Optional production flag to apply EF Core migrations on startup. |
| `Seed__RunOnStartup` | Optional production-safe one-time simulated seed flag. Disable after seeding. |
| `Google__ClientId` | Google OAuth client ID. |
| `Google__ClientSecret` | Google OAuth client secret. |
| `FrontendUrl` | Browser-facing frontend URL, locally `http://localhost:5173`, deployed `https://setlist-social.vercel.app`. |
| `LastFm__ApiKey` | Last.fm API key for artist search. |
| `E2E__EnableTestAuth` | Development/test-only flag used by Playwright; never enable in production. |

Frontend:

| Variable | Description |
| --- | --- |
| `VITE_API_BASE_URL` | Backend API origin for local development. In deployed Vercel, same-origin rewrites are used. |

## Creating Local API Credentials

Do not use or commit another developer's credentials. To run the full app locally, create your own Google OAuth client and Last.fm API key.

Google OAuth/OIDC:

1. Open Google Cloud Console and create or select a project.
2. Configure the OAuth consent screen. If the app is in testing mode, add your Google email as a test user.
3. Create an OAuth client ID with application type **Web application**.
4. Add this local redirect URI:

```text
http://localhost:5050/api/auth/google-callback
```

5. If also testing the deployed Vercel flow, add the deployed callback URI:

```text
https://setlist-social.vercel.app/api/auth/google-callback
```

6. Copy the generated client ID and client secret into your local backend startup command as `Google__ClientId` and `Google__ClientSecret`.

Last.fm:

1. Create or sign in to a Last.fm account.
2. Create an API application/key from the Last.fm API account area.
3. Copy the API key into your local backend startup command as `LastFm__ApiKey`.

These values are runtime environment variables only. Do not place real values in `README.md`, `appsettings.Development.json`, `.env`, committed scripts, or source code.

## Local Setup

Install frontend dependencies:

```bash
cd frontend
npm install
```

Run the backend locally:

```bash
cd backend
dotnet restore
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5050 \
Google__ClientId="YOUR_GOOGLE_CLIENT_ID" \
Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET" \
FrontendUrl="http://localhost:5173" \
LastFm__ApiKey="YOUR_LASTFM_API_KEY" \
dotnet run
```

Run the frontend locally in another terminal:

```bash
cd frontend
VITE_API_BASE_URL="http://localhost:5050" npm run dev
```

Useful local URLs:

- Frontend: `http://localhost:5173`
- Backend health: `http://localhost:5050/api/health`
- Swagger UI: `http://localhost:5050/swagger`

## Database, Migrations, And Seed Data

Setlist Social uses EF Core for persistence.

Local development uses SQLite by default through `backend/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=setlist-social-dev.db"
  }
}
```

The generated SQLite `.db` file is ignored by Git and should not be committed.

Production uses PostgreSQL through the `ConnectionStrings__DefaultConnection` environment variable. The backend chooses the provider from `Database__Provider` when set, otherwise it uses PostgreSQL for production/PostgreSQL-style connection strings and SQLite for local development. Do not put real production connection strings in source control.

To apply EF Core migrations locally, run this from the repository root:

```bash
dotnet ef database update --project backend/SetlistSocial.Api.csproj --startup-project backend/SetlistSocial.Api.csproj
```

Then start the backend and run one of the development seed commands below.

Small development seed:

```bash
curl -X POST "http://localhost:5050/api/dev/seed"
```

Full-scale development seed/reset:

```bash
curl -X POST "http://localhost:5050/api/dev/seed/full?reset=true"
```

The small seed creates a few sample users, artists, concerts, reviews, wishlist items, activity events, and tags. The full-scale seed creates deterministic simulated data with 500+ user profiles, 5,000+ domain records, and 10,000+ activity/interactions using curated local artist lists. The full-scale reset option is for local development only.

For production on Render, migrations and seeding are explicit opt-in startup actions:

```text
Database__RunMigrationsOnStartup=true
Seed__RunOnStartup=true
```

`Database__RunMigrationsOnStartup=true` applies EF Core migrations on backend startup. `Seed__RunOnStartup=true` runs the production-safe simulated seed after migrations. Both are disabled by default. Production seeding is idempotent and skips if generated seed/domain data already exists. It does not drop, truncate, reset, or delete production data.

After production stats show seeded data, turn `Seed__RunOnStartup` back off in Render and redeploy/restart. Keep `Database__RunMigrationsOnStartup` off unless intentionally applying migrations for a deployment.

## Test Commands

Backend:

```bash
dotnet test backend.tests/SetlistSocial.Api.Tests.csproj
```

Frontend:

```bash
cd frontend
npm test
npm run build
```

E2E:

```bash
cd frontend
npm run e2e
```

GitHub Actions runs backend build/tests, frontend tests/build, and Playwright E2E with a PostgreSQL service and `E2E__EnableTestAuth=true`.

## Known Limitations

- Last.fm integration is limited to public artist search; Last.fm user accounts and scrobbles are not connected.
- Profile and settings pages are intentionally lightweight.
- Reviews exist in the model and seed data, but full review CRUD is not implemented.
- The public SignalR activity hub broadcasts safe display data only; it is not a private notification system.
- The app relies on Google OAuth for identity but still enforces authorization in the backend for user-owned data.
- Rate limiting and production security headers are not fully implemented.

## Secrets Policy

No real credentials, database URLs, API keys, or OAuth secrets should be committed. Local secrets are passed as environment variables, and production secrets are configured in Vercel/Render dashboards.
