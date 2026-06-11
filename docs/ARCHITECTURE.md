# Architecture

## Status

The repository contains a working starter architecture with public pages, protected user-owned concert CRUD, and a small Last.fm artist search integration.

## Current Architecture

- Frontend: React + Vite + JavaScript
- Frontend routing: React Router
- Backend: ASP.NET Core Minimal API targeting .NET 10
- API documentation: Swagger/OpenAPI via Swashbuckle
- Auth: Google OAuth/OIDC foundation with cookie authentication
- ORM/database: EF Core with SQLite local development and PostgreSQL provider support
- External API: Last.fm artist search through the backend only
- Current external endpoint: `GET /api/external/lastfm/search?artist=ARTIST_NAME`

## Planned Architecture

- Auth provider: Google OAuth / OIDC
- ORM: EF Core
- Local development database: SQLite
- Production database: PostgreSQL
- Real-time feature: SignalR live activity feed
- Planned frontend deployment: Vercel
- Planned backend deployment: Render
- Planned production database hosting: Neon or Render PostgreSQL

## Planned Request Flow

1. React frontend calls the ASP.NET Core API.
2. API validates authenticated requests through Google OAuth / OIDC.
3. API reads and writes application data through EF Core.
4. API calls Last.fm for public artist search without exposing the API key to the frontend.
5. Planned: SignalR pushes live activity updates to connected frontend clients.

## Current Gaps

- Full protected CRUD is not implemented beyond My Concerts.
- Last.fm is limited to public artist search.
- SignalR is not implemented.
- Deployment is not configured.
