# Architecture

## Status

Structured placeholder. The repository currently contains minimal starter projects only.

## Current Architecture

- Frontend: React + Vite + JavaScript
- Frontend routing: React Router
- Backend: ASP.NET Core Minimal API targeting .NET 10
- API documentation: Swagger/OpenAPI via Swashbuckle
- Current endpoint: `GET /api/health`

## Planned Architecture

- Third-party API: Last.fm
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
4. API calls Last.fm for music-related data where needed.
5. SignalR pushes live activity updates to connected frontend clients.

## Current Gaps

- Authentication is not implemented.
- EF Core is not configured.
- Database schema and models are not created.
- Last.fm integration is not implemented.
- SignalR is not implemented.
- Deployment is not configured.
