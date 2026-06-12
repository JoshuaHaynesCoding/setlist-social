# Setlist Social Frontend

React + Vite + JavaScript frontend for Setlist Social.

## Current Routes

- `/` - landing page
- `/about` - about page
- `/stats` - community stats from the backend
- `/artists` - public artists from the backend
- `/activity` - recent public activity from the backend with SignalR live updates
- `/discover` - public Last.fm artist search
- `/dashboard` - protected current-user dashboard
- `/profile` - protected profile summary
- `/my-concerts` - protected user-owned concert CRUD
- `/wishlist` - protected user-owned wishlist
- `/settings` - protected account settings area

## Run Frontend And Backend Together

Before starting the backend, create your own local credentials:

- Google OAuth web application client with redirect URI `http://localhost:5050/api/auth/google-callback`.
- Last.fm API key from the Last.fm API account area.

Pass those values as `Google__ClientId`, `Google__ClientSecret`, and `LastFm__ApiKey` in the backend startup command. Do not commit real values.

From `backend/`, apply migrations and start the API:

```bash
dotnet restore
dotnet ef database update
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5050 \
Google__ClientId="YOUR_GOOGLE_CLIENT_ID" \
Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET" \
FrontendUrl="http://localhost:5173" \
LastFm__ApiKey="YOUR_LASTFM_API_KEY" \
dotnet run
```

Optional local sample data:

```bash
curl -X POST http://localhost:5050/api/dev/seed
```

From `frontend/`, install dependencies and start Vite:

```bash
npm install
VITE_API_BASE_URL="http://localhost:5050" npm run dev
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

The navbar sign-in control links to:

```text
http://localhost:5050/api/auth/login
```

In deployed Vercel, API and SignalR traffic use same-origin rewrites from `/api/*` and `/hubs/*` to the Render backend.

## Known Limitations

- Last.fm user account or scrobble integration
- Full profile/settings editing
- Review CRUD UI
