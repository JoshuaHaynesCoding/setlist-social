# Setlist Social

Setlist Social is a class final project in the Music & Events domain. The planned app will help users discover, discuss, and share concert setlists and music event activity.

This repository currently contains a minimal starter frontend, backend, and documentation structure. Most product features are still planned and are not implemented yet.

## Repository Structure

- `frontend/` - React + Vite + JavaScript starter app with React Router installed
- `backend/` - ASP.NET Core Minimal API targeting .NET 10 with Swagger/OpenAPI and `GET /api/health`
- `docs/` - project documentation placeholders and planning notes

## Current Implementation

- Frontend: minimal React + Vite app
- Routing: React Router installed and configured with starter routes
- Backend: ASP.NET Core Minimal API targeting .NET 10
- API docs: Swagger/OpenAPI configured
- Health check: `GET /api/health` returns `{ "status": "ok" }`

## Planned Features

- Third-party music API integration: Last.fm
- Authentication: Google OAuth / OIDC
- Database: SQLite for local development, PostgreSQL for production
- ORM: EF Core
- Real-time feature: SignalR live activity feed
- Deployment: Vercel for frontend, Render for backend, Neon or Render PostgreSQL for production database

## Local Development

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Backend:

```bash
cd backend
dotnet restore
dotnet run
```

Health check:

```bash
curl http://localhost:5050/api/health
```

Swagger UI:

```text
http://localhost:5050/swagger
```

## Notes

- No OAuth implementation has been added yet.
- No EF Core setup or database models have been added yet.
- No secrets or deployed URLs are stored in this repository.
