# Design Note

## Status

Early product/design notes for the current Setlist Social demo foundation.

## Product Context

- App name: Setlist Social
- Domain: Music & Events
- Planned audience: music fans, concertgoers, and users interested in setlist activity

## Current UI

- Public React routes for landing, about, stats, artists, activity, and discovery
- Protected signed-in foundations for dashboard, profile, my concerts, wishlist, and settings
- Simple card/list layouts intended to stay readable with larger local development data
- Public activity page with initial loading plus live update connection status

## Simulated Taste Groups

Full-scale development seed data uses simulated listener taste groups so demo activity feels more realistic without using real user data or external API calls. The current groups are hip-hop, R&B/soul, classic rock, hard rock/metal, electronic, indie/alternative, pop, and jazz.

Seed artists come from the curated local list at `docs/SEED_ARTIST_LISTS.txt`, while users and interactions are generated locally.

These groups are for local development behavior only. They are not a finished recommendation system or a production personalization model.

## Planned Design Direction

- Prioritize fast discovery of artists, events, and setlist activity
- Keep live activity visible but restrained, with clear connected/unavailable status
- Keep the interface clear enough for a class final project demo
- Use accessible contrast, semantic layout, and responsive views

## Open Design Questions

- What is the primary first screen after login?
- Should the app center on artists, events, setlists, or social activity first?
- What data from Last.fm will be shown in the initial demo?
- How should the live activity feed appear without overwhelming the main workflow?
