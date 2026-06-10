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

- OAuth is present only as a foundation; protected product workflows are not implemented.
- No production security headers or rate limiting are configured.
- Production CORS and cookie settings still need deployment-specific review.
