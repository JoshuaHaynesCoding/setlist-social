# Setlist Social Frontend

React + Vite + JavaScript public frontend for Setlist Social.

## Current Routes

- `/` - landing page
- `/about` - about page
- `/stats` - community stats from the backend
- `/artists` - public artists from the backend
- `/activity` - recent public activity from the backend

## Run Frontend And Backend Together

From `backend/`, apply migrations and start the API:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Optional local sample data:

```bash
curl -X POST http://localhost:5050/api/dev/seed
```

From `frontend/`, install dependencies and start Vite:

```bash
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

The public data pages call:

- `http://localhost:5050/api/public/stats`
- `http://localhost:5050/api/public/artists`
- `http://localhost:5050/api/public/activity`

If the backend is not running, these pages show an error state.

## Not Implemented Yet

- OAuth or protected routes
- Last.fm integration
- SignalR live updates
- Production API URL configuration
- Secrets
