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
- Non-owned concert ids return `404 Not Found` instead of `403 Forbidden` to avoid revealing whether another user's concert exists.
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

- OAuth is present as a foundation; the first user-owned concerts CRUD flow exists, but other protected CRUD workflows are not implemented.
- No production security headers or rate limiting are configured.
- Production CORS and cookie settings still need deployment-specific review.
