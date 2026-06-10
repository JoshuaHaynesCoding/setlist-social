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
- Google OAuth/OIDC foundation with cookie authentication
- Current user endpoint: `GET /api/me`

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

## Not Implemented Yet

- Last.fm integration
- SignalR
- Production PostgreSQL configuration
- Protected CRUD workflows
