# Setlist Social

CI badge placeholder: add the GitHub Actions badge here after the repository path is finalized and the workflow has run on GitHub.

Setlist Social is a class final project in the Music & Events domain. The planned app will help users discover, discuss, and share concert setlists and music event activity.

This repository contains the current Setlist Social frontend, backend, tests, and documentation structure. Some product features are implemented as class-project foundations, while production deployment and broader workflows are still planned.

## Repository Structure

- `frontend/` - React + Vite + JavaScript app with React Router, public pages, protected route foundations, Vitest tests, and Playwright E2E tests
- `backend/` - ASP.NET Core Minimal API targeting .NET 10 with Swagger/OpenAPI, EF Core, Google OAuth foundation, Last.fm artist search, SignalR activity updates, and backend tests
- `docs/` - project documentation and planning notes

## Current Implementation

- Frontend: React + Vite public/protected route foundation
- Routing: React Router
- Backend: ASP.NET Core Minimal API targeting .NET 10
- API docs: Swagger/OpenAPI configured
- Health check: `GET /api/health` returns `{ "status": "ok" }`
- Database: EF Core with SQLite local development and PostgreSQL production configuration
- Auth: Google OAuth/OIDC foundation with cookie auth
- External API: Last.fm artist search through the backend
- Real-time: SignalR public activity updates
- Tests: backend integration tests, frontend Vitest tests, and Playwright E2E tests

## Planned Features

- Deeper Last.fm features
- Broader protected CRUD workflows
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

## Continuous Integration

GitHub Actions workflow: `.github/workflows/ci.yml`

The workflow runs on pushes and pull requests to `main`.

Backend CI commands:

```bash
dotnet restore backend/SetlistSocial.Api.csproj
dotnet restore backend.tests/SetlistSocial.Api.Tests.csproj
dotnet build backend/SetlistSocial.Api.csproj --no-restore
dotnet test backend.tests/SetlistSocial.Api.Tests.csproj --no-restore
```

Frontend CI commands:

```bash
cd frontend
npm ci
npm test
npm run build
```

E2E CI commands:

```bash
dotnet restore backend/SetlistSocial.Api.csproj
dotnet tool install --global dotnet-ef --version 10.*
dotnet ef database update --project backend/SetlistSocial.Api.csproj --startup-project backend/SetlistSocial.Api.csproj
cd frontend
npm ci
npx playwright install --with-deps chromium
npm run e2e
```

The E2E job uses `E2E__EnableTestAuth=true` with a local SQLite database on the CI runner. It does not require Google OAuth credentials or other secrets.

## Deployment Configuration

No production secrets or deployed URLs are committed to this repository. Configure these values in the hosting provider dashboards.

Backend environment variables:

```text
ConnectionStrings__DefaultConnection
Database__RunMigrationsOnStartup
Google__ClientId
Google__ClientSecret
FrontendUrl
LastFm__ApiKey
```

Frontend environment variables:

```text
VITE_API_BASE_URL
```

Production notes:

- Local development defaults to SQLite through `backend/appsettings.Development.json`.
- Production uses PostgreSQL through `ConnectionStrings__DefaultConnection`.
- The backend reads Render's `PORT` environment variable when present.
- Set `Database__RunMigrationsOnStartup=true` on Render if you want the backend to apply EF Core migrations when the service starts.
- Do not run development seed/reset endpoints in production; they are only mapped in `Development`.
- Apply EF Core migrations to the production PostgreSQL database as a deliberate deployment step.

## Notes

- No secrets or deployed URLs are stored in this repository.
- Production hosting is prepared but has not been pushed/deployed from this repository yet.
