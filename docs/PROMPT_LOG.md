# Prompt Log

This file records major AI-assistance sessions for the Setlist Social project. It does not list every small prompt.

## 2026-06-09 - Initial Project Structure

- Tool: ChatGPT/Codex
- Goal: Create the initial repository structure only.
- Prompt: Create `frontend/`, `backend/`, `docs/`, `README.md`, `PRODUCT_BRIEF.md`, and a `.gitignore` for Node, .NET, SQLite, env files, build output, and dependencies.
- Result: Created the starter folder/document structure and ignore rules.
- Accepted: Basic repo layout and placeholder docs.
- Changed: Added top-level README, product brief placeholder, folder README placeholders, and `.gitignore`.
- Rejected: OAuth, database models, secrets, and app implementation.
- Tested: Verified the expected files existed.

## 2026-06-09 - Frontend And Backend Starter

- Tool: ChatGPT/Codex
- Goal: Create minimal runnable starter projects.
- Prompt: Add a React + Vite + JavaScript frontend with React Router and an ASP.NET Core Minimal API backend targeting .NET 10 with Swagger and `GET /api/health`.
- Result: Added Vite frontend files, React Router starter routes, backend project file, Swagger setup, launch settings, and health endpoint.
- Accepted: Minimal frontend/backend scaffold and dependency installation.
- Changed: Added `frontend/package.json`, `vite.config.js`, React source files, backend `.csproj`, `Program.cs`, and launch settings.
- Rejected: OAuth, EF Core, database models, secrets, and broader app features.
- Tested: `npm run build` passed; backend initially required .NET 10 SDK before it could build.

## 2026-06-10 - Documentation Skeleton

- Tool: ChatGPT/Codex
- Goal: Add structured documentation placeholders.
- Prompt: Create design, architecture, prompt log, AI reflection, security review, and accessibility report docs; update README and product brief with current project decisions.
- Result: Added required docs and marked planned features accurately.
- Accepted: Current/planned split for Last.fm, Google OAuth/OIDC, EF Core, SignalR, SQLite/PostgreSQL, and deployment.
- Changed: Added documentation files under `docs/`, updated `README.md`, and updated `PRODUCT_BRIEF.md`.
- Rejected: Fake deployed URLs, secrets, and claims that planned features were complete.
- Tested: Reviewed docs for inaccurate completion claims.

## 2026-06-10 - EF Core Models And Migration

- Tool: ChatGPT/Codex
- Goal: Add the first backend domain/data layer.
- Prompt: Add EF Core SQLite/PostgreSQL provider support, configure local SQLite, create `AppDbContext`, add domain entities and relationships, add stats endpoint, seed foundation, and create the first migration if possible.
- Result: Added EF Core packages, domain models, DbContext, SQLite configuration, initial migration, and `/api/public/stats`.
- Accepted: `UserProfile`, `Artist`, `Concert`, `Review`, `WishlistItem`, `ActivityEvent`, `Tag`, and the initial migration.
- Changed: Added backend data/model files, `appsettings.Development.json`, migration files, package references, and backend README migration notes.
- Rejected: OAuth, Last.fm, SignalR, frontend UI, production DB setup, secrets, and `EnsureCreated` in production code.
- Tested: `dotnet build`, `dotnet ef migrations list`, and `dotnet ef database update` passed.

## 2026-06-10 - Development Seed Endpoint

- Tool: ChatGPT/Codex
- Goal: Add small local sample data for development.
- Prompt: Add a development-only `POST /api/dev/seed` endpoint that creates a small dataset and prevents duplicate seeding.
- Result: Seed endpoint creates sample users, artists, concerts, reviews, wishlist items, activity events, and tags when the database is empty.
- Accepted: Development-only guard and duplicate prevention.
- Changed: Updated backend seed logic and backend README seed instructions.
- Rejected: Full large-scale seed data, OAuth, Last.fm, SignalR, frontend UI, and secrets.
- Tested: `dotnet build` passed.

## 2026-06-10 - Public Frontend Routes And Stats

- Tool: ChatGPT/Codex
- Goal: Build the first public React frontend.
- Prompt: Add public routes for landing, about, stats, artists, and activity; create reusable components; fetch stats from the backend; add development CORS only if needed.
- Result: Added polished public React routes, shared layout/components, stats fetch states, and development CORS for Vite.
- Accepted: `Layout`, `Navbar`, `StatCard`, `LoadingState`, `ErrorState`, `EmptyState`, and `/stats` backend fetch.
- Changed: Added frontend pages/components/styles, frontend README, and backend development CORS.
- Rejected: OAuth, protected routes, Last.fm, SignalR, secrets, and full protected UI.
- Tested: `npm run build` and `dotnet build` passed; local server binding was limited by the sandbox.

## 2026-06-10 - Public Artists And Activity Pages

- Tool: ChatGPT/Codex
- Goal: Use real backend data on public Artists and Activity pages.
- Prompt: Add public artists and activity endpoints using DTOs, then update `/artists` and `/activity` to fetch and display them with loading/error/empty states.
- Result: Added `GET /api/public/artists` and `GET /api/public/activity`; updated frontend pages to display cards/lists.
- Accepted: Public DTO endpoints and consistent frontend data states.
- Changed: Updated `Program.cs`, artists/activity pages, styles, and frontend README.
- Rejected: Schema changes, new migration, OAuth, protected routes, Last.fm, SignalR, and secrets.
- Tested: `dotnet build` and `npm run build` passed.

## 2026-06-10 - Google OAuth Foundation

- Tool: ChatGPT/Codex
- Goal: Add the first safe OAuth/OIDC foundation.
- Prompt: Add Google OAuth/OIDC auth support using configuration/environment variables, add login/callback/logout/me endpoints, create or update `UserProfile` on first login, and add a simple frontend sign-in control.
- Result: Added Google auth package, cookie authentication, auth endpoints, `/api/me`, user profile upsert, and basic frontend signed-in state check.
- Accepted: Configuration-only `Google__ClientId`, `Google__ClientSecret`, and `FrontendUrl`; no credentials committed.
- Changed: Updated backend auth setup, frontend auth status component/navbar, styles, backend/frontend docs, security review, and prompt log.
- Rejected: Last.fm, SignalR, protected CRUD, hardcoded credentials, exposed secrets, and full protected UI.
- Tested: `dotnet build` and `npm run build` passed.

## 2026-06-10 - `/api/me` 401 Fix

- Tool: ChatGPT/Codex
- Goal: Make signed-out API behavior correct.
- Prompt: Fix `/api/me` so signed-out requests return `401 Unauthorized` instead of redirecting to Google login, while keeping `/api/auth/login` working.
- Result: Removed the pre-endpoint authorization challenge from `/api/me` so its own unauthenticated check returns `401`.
- Accepted: Narrow endpoint behavior fix.
- Changed: Updated `backend/Program.cs`.
- Rejected: New features, secrets, and broader auth rewrites.
- Tested: `dotnet build` passed.

## 2026-06-10 - Protected Route And Dashboard Foundation

- Tool: ChatGPT/Codex
- Goal: Add the first signed-in frontend area without full CRUD.
- Prompt: Add protected frontend routes and a simple signed-in dashboard foundation; add `GET /api/me/dashboard` for the current user only.
- Result: Added a user-scoped dashboard endpoint, shared frontend auth context, protected route wrapper, signed-in dashboard, and placeholder protected pages.
- Accepted: `/dashboard`, `/profile`, `/my-concerts`, `/wishlist`, `/settings`, and current-user dashboard counts.
- Changed: Updated backend auth endpoints, frontend routing/auth components/pages/styles, security review, and prompt log.
- Rejected: Full CRUD, Last.fm, SignalR, production deployment, secrets, and cross-user dashboard access.
- Tested: `dotnet build` and `npm run build` passed.

## 2026-06-10 - User-Owned My Concerts CRUD

- Tool: ChatGPT/Codex
- Goal: Add the first protected user-owned CRUD flow.
- Prompt: Add signed-in user's concert CRUD endpoints and replace `/my-concerts` with a protected create/edit/delete UI.
- Result: Added current-user-only concert API endpoints, validation, DTO responses, and a protected frontend list/form flow.
- Accepted: `GET/POST/PUT/DELETE /api/me/concerts`, user isolation, simple validation, and real My Concerts UI.
- Changed: Updated `Program.cs`, `MyConcertsPage.jsx`, styles, security review, and prompt log.
- Rejected: Last.fm, SignalR, deployment, secrets, full-scale seed, and cross-user concert access.
- Tested: `dotnet build` and `npm run build` passed.

## 2026-06-10 - Backend Auth And User Isolation Tests

- Tool: ChatGPT/Codex
- Goal: Add backend tests for protected API behavior and My Concerts user isolation.
- Prompt: Create an xUnit integration test project using a test auth scheme to verify unauthenticated `401` responses and current-user-only concert access.
- Result: Added backend integration tests with an in-memory SQLite database and test-only authentication.
- Accepted: Tests for `/api/me`, `/api/me/dashboard`, `/api/me/concerts`, signed-in concert create/read, and cross-user read/update/delete denial.
- Changed: Added `backend.tests`, exposed `Program` for test hosting, and updated security/prompt docs.
- Rejected: Real OAuth testing, production auth changes, Last.fm, SignalR, deployment, secrets, and new product features.
- Tested: `dotnet test backend.tests/SetlistSocial.Api.Tests.csproj` passed.

## 2026-06-10 - Last.fm Artist Search Integration

- Tool: ChatGPT/Codex
- Goal: Add the first third-party API integration without exposing secrets.
- Prompt: Add Last.fm `artist.search` through the backend and a public Discover page that searches artists.
- Result: Added backend Last.fm client, public search endpoint, frontend `/discover` route, search form, result cards, and setup docs.
- Accepted: `LastFm:ApiKey` configuration via user-secrets, clean DTOs, missing-key `503` response, and public Last.fm data display.
- Changed: Added backend external client files, Discover page, navbar route, styles, backend README, architecture doc, and prompt log.
- Rejected: Last.fm user account/scrobble integration, SignalR, deployment, secrets, wishlist/concert actions from search, and full-scale seed.
- Tested: `dotnet build` and `npm run build` passed.

## 2026-06-10 - GitHub Copilot Accessibility Review

- Tool: GitHub Copilot
- Goal: Review `AuthStatus.jsx` and `Navbar.jsx` for focused accessibility and UX improvements.
- Prompt: Review `frontend/src/components/AuthStatus.jsx` and `frontend/src/components/Navbar.jsx` for accessibility and UX issues; suggest small improvements only.
- Result: Copilot suggested `aria-live`/`role=status`, `displayName` fallback, logout loading state, and semantic nav list structure.
- Accepted: Targeted accessibility/UX changes.
- Changed: Updated auth status messaging, fallback display text, logout disabled/loading behavior, and nav semantics.
- Rejected: Full mobile menu, layout rewrite, and unrelated frontend changes.
- Tested: Sign-in/sign-out, `/api/me` signed in and signed out, and `npm run build`.
