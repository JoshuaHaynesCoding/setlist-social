# Security Review

## OAuth/OIDC Flow

Setlist Social uses Google OAuth/OIDC through ASP.NET Core authentication middleware. `GET /api/auth/login` starts the Google challenge. Google returns to the configured callback path `/api/auth/google-callback`, which is handled by the Google middleware. ASP.NET Core's remote authentication handler manages the OAuth correlation/state validation before the app-level callback creates or updates the local `UserProfile`. If the remote login fails, the user is not signed in.

The app-level callback then redirects to `FrontendUrl`. In production, Vercel is the browser-facing frontend origin and Render is the backend origin behind API rewrites. Google callback URLs must match the deployed browser-facing callback configuration.

## Token And Session Storage

Google tokens are not saved into the application auth cookie because `SaveTokens` is disabled. The browser stores an application session cookie named `setlist_social_auth`. The cookie is HTTP-only, so frontend JavaScript cannot read it. In production the cookie is configured with `SameSite=None` and `SecurePolicy=Always` so it can work with the deployed frontend/backend flow over HTTPS. The tradeoff is that cookie-based auth requires correct CORS, HTTPS, and credentialed fetch configuration.

Signing out clears the app cookie through `POST /api/auth/logout`, and the frontend redirects back to the home page. Signing out of Setlist Social does not sign the user out of Google globally; Google maintains its own separate session cookies on Google domains.

## Protected Routes

Frontend route guards improve user experience, but they are not the security boundary. The backend independently enforces authentication for `/api/me`, `/api/me/dashboard`, `/api/me/concerts`, and `/api/me/wishlist`. If a user is signed out, protected API endpoints return `401 Unauthorized`.

## Authorization And User Isolation

User isolation is enforced with the authenticated Google subject. The backend looks up the current `UserProfile` from the claims principal, then checks ownership before returning or mutating user-owned records. A user cannot request another user's dashboard. For concerts and wishlist items, replacing an ID in a URL does not grant access. If the record exists but belongs to another user, the backend returns `403 Forbidden`. If the record does not exist, the backend returns `404 Not Found`.

Direct backend integration tests confirm:

- signed-out `/api/me`, `/api/me/dashboard`, `/api/me/concerts`, and wishlist endpoints return `401`;
- a signed-in user can create and read their own concert;
- a second signed-in user receives `403` for another user's concert read/update/delete;
- a second signed-in user receives `403` when deleting another user's wishlist item.

## Input Validation

Validation is enforced server-side for create/update inputs. Concert requests validate required title, artist name, concert date, and maximum lengths for venue/city/region/country fields. Wishlist requests validate required artist name, maximum lengths for source fields, and absolute URL format for source URLs. Last.fm search validates that the `artist` query is not empty and returns validation errors for missing input. The frontend also uses required form fields and user-friendly messages, but backend validation is the source of truth.

## Error Handling

The backend uses `401`, `403`, `404`, `409`, validation problem responses, and problem responses for service configuration failures. Recent backend polish added OpenAPI response metadata for the main endpoints. Error messages avoid exposing OAuth secrets, database connection strings, Last.fm keys, or internal auth identifiers. Some error shapes are still mixed between status-only responses and problem responses, which is a known limitation.

## CORS

CORS allows the configured `FrontendUrl`, the deployed Vercel origin `https://setlist-social.vercel.app`, and local development origins in `Development`. Credentialed requests are allowed with `AllowCredentials()`. The backend does not use `AllowAnyOrigin()` with credentials.

## HTTPS And Forwarded Headers

Production traffic uses public HTTPS through Vercel and Render. The backend processes forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`) so ASP.NET can understand the original request context behind the hosting proxy.

## Secrets Management

Secrets are not committed to the repository. Local runs inject secrets as environment variables in the startup command. Render and Vercel store production secrets in their dashboards. Required secret/config values include `Google__ClientId`, `Google__ClientSecret`, `FrontendUrl`, `LastFm__ApiKey`, and `ConnectionStrings__DefaultConnection`.

An AI-generated version previously used a fallback expression that turned missing Google OAuth config into an empty string. That produced confusing `invalid_client` errors later in the OAuth flow. The code now fails fast with a clear error if `Google:ClientId` or `Google:ClientSecret` is missing or blank.

## Dependency Vulnerability Checks

Commands run on 2026-06-12:

```bash
cd frontend
npm audit --audit-level=moderate
```

Result: `found 0 vulnerabilities`.

```bash
dotnet list backend/SetlistSocial.Api.csproj package --vulnerable
dotnet list backend.tests/SetlistSocial.Api.Tests.csproj package --vulnerable
```

Result: both projects reported no vulnerable packages from the current NuGet sources.

## Known Security Limitations

- OAuth proves identity, but it does not by itself protect user data; backend ownership checks are still required and implemented for current user-owned concerts and wishlist items.
- Rate limiting is not implemented.
- A full production security header policy is not implemented.
- Last.fm integration is app-level artist search only; there is no Last.fm account linking or scrobble access.
- Public activity is intentionally public and should never include private fields.
- The app does not provide admin moderation tools.
- Some protected pages are lightweight and do not yet expose full profile/settings controls.
