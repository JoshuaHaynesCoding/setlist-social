# Architecture

## Status

The repository contains a working starter architecture with public pages, protected user-owned concert and wishlist foundations, a small Last.fm artist search integration, and local development seed/reset tooling.

## Current Architecture

- Frontend: React + Vite + JavaScript
- Frontend routing: React Router
- Backend: ASP.NET Core Minimal API targeting .NET 10
- API documentation: Swagger/OpenAPI via Swashbuckle
- Auth: Google OAuth/OIDC foundation with cookie authentication
- ORM/database: EF Core with SQLite local development and PostgreSQL provider support
- External API: Last.fm artist search through the backend only
- Current external endpoint: `GET /api/external/lastfm/search?artist=ARTIST_NAME`
- Development data: deterministic simulated local seed data for demo/testing scale

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

## Development Seed Data

The backend includes development-only seed endpoints. The small seed creates a tiny sample dataset, while the full-scale seed/reset endpoint creates simulated local data for demo and testing scale without using real users or calling Last.fm.

The full-scale seed reads real artist names from the curated local file at `docs/SEED_ARTIST_LISTS.txt`, grouped by hip-hop, R&B/soul, classic rock, hard rock/metal, electronic, indie/alternative, pop, and jazz. User profiles, concerts, reviews, wishlist rows, and activity events are still simulated. Users in a taste group are more likely to save, review, and attend artists from that same group.

## Current Gaps

- Full protected CRUD is not implemented beyond the current My Concerts and Wishlist foundations.
- Last.fm is limited to public artist search.
- SignalR is not implemented.
- Deployment is not configured.
