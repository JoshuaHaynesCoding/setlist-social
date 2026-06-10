# Setlist Social Product Brief

## Overview

Setlist Social is a class final project in the Music & Events domain. The planned product will let music fans track concert setlists, connect around live music activity, and see updates from other users.

## Problem Statement

Planned: Music fans often discover setlists, artist activity, and event conversation across disconnected services. Setlist Social will explore a focused social experience around concerts, artists, and setlist-related activity.

## Target Users

- Concertgoers who want to follow artists and events
- Music fans who want to discuss recent or upcoming shows
- Users who want lightweight social activity around setlists

## Current Scope

- Minimal React + Vite + JavaScript frontend starter
- React Router installed and configured
- ASP.NET Core Minimal API backend targeting .NET 10
- Swagger/OpenAPI configured
- Public `GET /api/health` endpoint returning `{ "status": "ok" }`
- Documentation placeholders created for design, architecture, AI usage, security, and accessibility

## Planned Scope

- Last.fm integration for music data
- Google OAuth / OIDC authentication
- EF Core data access
- SQLite database for local development
- PostgreSQL database for production
- SignalR live activity feed
- Deployment to Vercel, Render, and Neon or Render PostgreSQL

## Out Of Scope For Current Starter

- OAuth implementation
- Database models
- EF Core setup
- Last.fm API calls
- SignalR implementation
- Production deployment
- Secrets or environment-specific credentials
