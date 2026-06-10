# Security Review

## Status

Structured placeholder. Security implementation work has not started.

## Current Security State

- No OAuth implementation exists yet.
- No database or data models exist yet.
- No secrets are stored in the repository.
- `.env` files are ignored by `.gitignore`.
- The only backend route is public `GET /api/health`.

## Planned Security Areas

- Google OAuth / OIDC authentication
- Secure server-side validation of authenticated users
- Environment-based configuration for secrets
- Last.fm API key handling through environment variables
- Database connection string protection
- Production CORS configuration
- Input validation for API endpoints

## Open Security Questions

- Which Google OAuth client settings are required for local and deployed environments?
- What user profile data will be stored?
- Which endpoints will be public versus authenticated?
- What CORS origins will be allowed in production?

## Known Current Limitations

- Authentication and authorization are not implemented.
- No production security headers or rate limiting are configured.
- No persistence layer exists yet.
