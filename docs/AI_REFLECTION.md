# AI Reflection

## How AI Accelerated The Work

1. Codex accelerated project scaffolding by creating the initial React/Vite and ASP.NET Core Minimal API structure quickly, including folders, starter docs, `.gitignore`, and basic build/test setup. This let the project move from blank repository to runnable frontend/backend faster than manual setup alone.

2. Codex improved implementation speed for repeated full-stack patterns. Once the first public stats route existed, similar patterns were reused for artists, activity, dashboard data, My Concerts, Wishlist, loading states, empty states, and error states. The AI was useful for keeping DTOs, fetch calls, and UI states consistent.

3. Copilot/Codex helped expand testing. The backend integration test project, frontend Vitest tests, and Playwright scenarios were all drafted with AI assistance. The tests gave fast feedback on protected routes, user isolation, wishlist duplicate handling, Last.fm error behavior, and critical E2E flows.

## AI-Generated Test Review

I reviewed AI-assisted tests rather than accepting them as final on the first pass.

1. `Another_signed_in_user_cannot_read_update_or_delete_a_concert_they_do_not_own`
   - Initial value: covered the most important user-isolation case for My Concerts.
   - Improvement: updated expected responses from `404 NotFound` to `403 Forbidden` after aligning the backend with the rubric requirement for cross-user boundaries.

2. `Another_signed_in_user_cannot_delete_a_wishlist_item_they_do_not_own`
   - Initial value: confirmed wishlist ownership was enforced.
   - Improvement: kept the owner re-read assertion so the test proves the other user did not delete the record, not just that a status code was returned.

3. `Wishlist_prevents_duplicate_artist_saves_for_the_same_user`
   - Initial value: checked duplicate handling.
   - Improvement: made the duplicate input use different casing and extra spaces so the test verifies trimmed, case-insensitive comparison rather than only exact-string matching.

4. `LastFm_search_uses_stubbed_client_for_successful_search`
   - Initial value: tested the third-party search endpoint.
   - Improvement: replaced real Last.fm calls with a stubbed `ILastFmClient`, keeping tests deterministic and avoiding API keys/network dependency.

5. Frontend `ProtectedRoute` unauthenticated test
   - Initial value: verified signed-out users could not see protected content.
   - Improvement: updated the test after the UX change from a "sign in required" panel to redirecting home, so it now matches actual app behavior.

6. Playwright signed-in concert CRUD scenario
   - Initial value: covered create, refresh, persistence, and delete in a browser.
   - Improvement: used a development/test-only auth endpoint instead of automating real Google OAuth, keeping E2E stable without weakening production auth.

## Where AI Was Wrong Or Misleading

1. The AI initially produced Google OAuth configuration code that quietly fell back to an empty string when `Google:ClientId` or `Google:ClientSecret` was missing. That made failures appear later as confusing Google `invalid_client` errors. I caught it by comparing hardcoded working values against configuration-loaded values, then changed the code to throw a clear startup/configuration error when required OAuth values are missing or empty.

2. The AI's early documentation overstated or understated the project state in different places. Some docs still said OAuth, EF Core, SignalR, and deployment were planned even after they were implemented. I caught this by comparing the rubric against the repository and updating docs to separate implemented features from real limitations.

3. The AI originally favored a custom dotenv loader. In practice, that was not the standard ASP.NET Core path I wanted, and it introduced confusion around whether secrets were loaded. I removed the custom loader and documented the simpler standard pattern: inject environment variables at process startup locally and through hosting provider dashboards in production.

4. The AI incorrectly assumed Render might still be running with `ASPNETCORE_ENVIRONMENT=Development` and suggested changing it to `Production`, even though Render was already configured for `Production`. That advice did not match the actual deployed state and distracted from the real OAuth issue.

5. The AI also pushed the Vercel `/api` proxy diagnosis too confidently before fully confirming ASP.NET Core's OAuth cookie and correlation behavior. That led to going in circles between direct Render auth and Vercel proxy auth. The better clue came from inspecting the actual Google error URL, which showed `%0A%0A` at the end of the `client_id`. That meant the configured Google Client ID had hidden newline characters, so Google was receiving an invalid client value even though the visible text looked correct.

## Human Architectural Decision

I chose to keep Last.fm integration server-side and limited to public artist search instead of connecting real Last.fm user accounts or scrobbles. That decision was not delegated to AI because it shaped the privacy and product boundary: Last.fm data is external discovery data, while Setlist Social wishlist/concert data is user-owned local app data. This kept the third-party API useful without adding unnecessary account-linking risk.

## Debugging Session

One important debugging session involved deployed Google OAuth. The frontend and backend were on different origins, and direct Render login did not reliably leave the Vercel frontend signed in. I had to understand the OAuth/cookie flow rather than just re-prompting. The fix involved using Vercel as the browser-facing origin for `/api/auth/login` and `/api/auth/google-callback`, configuring backend cookies/CORS for credentialed cross-origin use, and then later reverting parts of the proxy approach when ASP.NET correlation cookie behavior required a consistent login/callback host. That debugging clarified how app cookies, Google cookies, redirect URIs, and browser origins interact.

The final deployed OAuth fix came from reading the real Google failure URL instead of only reasoning from framework behavior. The URL-encoded `client_id` ended with `%0A%0A`, revealing hidden newline characters in the Render environment variable. I fixed the issue by cleaning the Google Client ID value in the Render dashboard so it contained only the exact client ID string, with no quotes, spaces, or trailing newlines, then restarting/redeploying the backend so ASP.NET loaded the corrected environment variable. After that, Google OAuth worked correctly in the deployed app.

Another debugging example was the sign-out behavior. Clearing the Setlist Social cookie correctly signed the user out of the app, but Google could still remember the account because Google owns separate cookies on Google domains. Understanding that distinction helped explain why signing back in could immediately reuse the same Google account.

## Prompting Strategy Evolution

Early prompts were broad: create structure, add frontend, add backend. As the project matured, effective prompts became narrower and more testable. I learned to specify what not to add, such as no OAuth during early data modeling, no EF Core during initial frontend work, and no secrets in any generated files. I also learned to ask for direct verification: run `dotnet build`, run `npm test`, run `npm run build`, and explain changed files. Later prompts focused on security boundaries, deployment behavior, and exact HTTP status expectations, such as requiring `403` for cross-user access. The strongest prompts described the desired behavior, the constraints, and the verification steps all together.

## Current Practice

AI output is treated as a draft. I review generated code against the rubric, test behavior locally or in CI, and update docs when the implementation changes. For security-sensitive areas such as OAuth secrets, cookies, authorization, and deployment environment variables, I rely on explicit code inspection and runtime testing rather than assuming the AI's first answer is correct.
