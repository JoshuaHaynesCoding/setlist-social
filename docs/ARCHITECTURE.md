# Architecture

## Status

The repository contains a working starter architecture with public pages, protected user-owned concert and wishlist foundations, a small Last.fm artist search integration, SignalR public activity updates, and local development seed/reset tooling.

## Current Architecture

- Frontend: React + Vite + JavaScript
- Frontend routing: React Router
- Backend: ASP.NET Core Minimal API targeting .NET 10
- API documentation: Swagger/OpenAPI via Swashbuckle
- Auth: Google OAuth/OIDC foundation with cookie authentication
- ORM/database: EF Core with SQLite local development and PostgreSQL production support
- External API: Last.fm artist search through the backend only
- Current external endpoint: `GET /api/external/lastfm/search?artist=ARTIST_NAME`
- Real-time: SignalR public activity hub at `/hubs/activity`
- Development data: deterministic simulated local seed data for demo/testing scale

## Planned Architecture

- Auth provider: Google OAuth / OIDC
- ORM: EF Core
- Local development database: SQLite
- Production database: PostgreSQL
- Planned frontend deployment: Vercel
- Planned backend deployment: Render
- Planned production database hosting: Neon or Render PostgreSQL

## Deployment Shape

The frontend is designed for Vercel with `VITE_API_BASE_URL` pointing at the deployed backend API origin. Local frontend development falls back to `http://localhost:5050`.

The backend is designed for Render. It can run from `backend/Dockerfile`, reads Render's `PORT` environment variable when present, and expects production configuration through environment variables rather than committed settings.

Database provider selection is environment-aware:

- Development/local default: SQLite through `ConnectionStrings__DefaultConnection` in `appsettings.Development.json`
- Production: PostgreSQL through `ConnectionStrings__DefaultConnection`

Production migrations are EF Core migrations. They can be applied manually, or the backend can apply them on startup when `Database__RunMigrationsOnStartup=true` is configured. That startup option is disabled by default and does not seed, reset, or drop data.

Development seed/reset endpoints are only mapped in `Development`, and full-scale simulated data is not seeded automatically on production startup.

Production can opt into the same simulated dataset with `Seed__RunOnStartup=true`. That path runs after the migration startup step, skips existing generated/domain data, preserves real OAuth users, and never resets or deletes production data.

## Current Request Flow

1. React frontend calls the ASP.NET Core API.
2. API validates authenticated requests through Google OAuth / OIDC.
3. API reads and writes application data through EF Core.
4. API calls Last.fm for public artist search without exposing the API key to the frontend.
5. SignalR pushes public-safe live activity updates to connected frontend clients.

## Public Activity Privacy

The public activity hub does not require authentication to connect. Broadcast payloads are limited to safe display fields: friendly activity type, message, display user handle/name, timestamp, and optional artist/concert display text. Private auth identifiers, OAuth subjects, emails, and private notes are not included.

## Development Seed Data

The backend includes development-only seed endpoints. The small seed creates a tiny sample dataset, while the full-scale seed/reset endpoint creates simulated local data for demo and testing scale without using real users or calling Last.fm.

The full-scale seed reads real artist names from the curated local file at `docs/SEED_ARTIST_LISTS.txt`, grouped by hip-hop, R&B/soul, classic rock, hard rock/metal, electronic, indie/alternative, pop, and jazz. User profiles, concerts, reviews, wishlist rows, and activity events are still simulated. Users in a taste group are more likely to save, review, and attend artists from that same group.

## Current Gaps

- Full protected CRUD is not implemented beyond the current My Concerts and Wishlist foundations.
- Last.fm is limited to public artist search.
- Deployment credentials, hosted service instances, and deployed URLs are not configured in the repository.
