# Setlist Social Frontend

React + Vite + JavaScript public frontend for Setlist Social.

## Current Routes

- `/` - landing page
- `/about` - about page
- `/stats` - community stats from the backend
- `/artists` - public artists from the backend
- `/activity` - recent public activity from the backend with SignalR live updates
- `/discover` - public Last.fm artist search

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

Run frontend unit tests:

```bash
npm test
```

Run Playwright end-to-end tests:

```bash
npm run e2e
```

The Playwright config starts the backend and frontend locally. For the signed-in E2E flow, the backend is started with `E2E__EnableTestAuth=true`, which enables a development/test-only login endpoint for automated tests.

Open:

```text
http://localhost:5173
```

The public data pages call:

- `http://localhost:5050/api/public/stats`
- `http://localhost:5050/api/public/artists`
- `http://localhost:5050/api/public/activity`
- `http://localhost:5050/api/external/lastfm/search?artist=ARTIST_NAME`
- `http://localhost:5050/hubs/activity`
- `http://localhost:5050/api/me`

If the backend is not running, these pages show an error state.

The navbar includes a lightweight sign-in control that links to:

```text
http://localhost:5050/api/auth/login
```

## Not Implemented Yet

- Last.fm user account or scrobble integration
- Production API URL configuration
- Secrets
