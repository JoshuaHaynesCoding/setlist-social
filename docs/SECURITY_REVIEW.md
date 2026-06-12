# Security Review

## Status

Initial OAuth/OIDC foundation has started. This is not a complete production security review.

## Current Security State

- Google OAuth/OIDC support is configured in the backend using environment-based configuration.
- Cookie authentication is configured for the current backend session.
- `GET /api/auth/login` starts Google login.
- `GET /api/auth/callback` creates or updates a local `UserProfile` after successful login.
- `POST /api/auth/logout` clears the auth cookie.
- `GET /api/me` returns the signed-in profile or `401` if unauthenticated.
- `GET /api/me/dashboard` returns only the current signed-in user's dashboard data or `401` if unauthenticated.
- `GET/POST/PUT/DELETE /api/me/concerts` endpoints are scoped to the current signed-in user's `UserProfile`.
- Non-owned concert ids return `403 Forbidden` to make cross-user authorization boundaries explicit.
- `GET/POST/DELETE /api/me/wishlist` endpoints are scoped to the current signed-in user's `UserProfile`.
- Non-owned wishlist item ids return `403 Forbidden` to make cross-user authorization boundaries explicit.
- Duplicate wishlist saves return `409 Conflict` for the signed-in user's own duplicate artist, without creating another row.
- Backend integration tests cover unauthenticated `401` responses and concert/wishlist user isolation with a test-only auth scheme.
- The frontend has protected route guards for dashboard/profile/concerts/wishlist/settings placeholder pages.
- Public endpoints remain public.
- No secrets are stored in the repository.
- `.env` files are ignored by `.gitignore`.
- Google tokens are not saved into the application auth cookie.

## Planned Security Areas

- Secure server-side validation of authenticated users
- Environment-based configuration for secrets
- Last.fm API key handling through environment variables
- Database connection string protection
- Production CORS configuration
- Input validation for API endpoints

## Open Security Questions

- Which deployed callback URLs will be registered with Google?
- What additional user profile data should be stored beyond display name and OAuth subject?
- Which endpoints will be public versus authenticated?
- What CORS origins will be allowed in production?

## Known Current Limitations

- OAuth is present as a foundation; user-owned concerts and wishlist flows exist, but other protected CRUD workflows are not implemented.
- No production security headers or rate limiting are configured.
- Production CORS and cookie settings still need deployment-specific review.
