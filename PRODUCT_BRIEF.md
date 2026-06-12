# Setlist Social Product Brief

## Overview

Setlist Social is a class final project in the Music & Events domain. The app helps music fans discover artists, save wishlist artists, track concerts they have attended or want to remember, and see public community activity around music events.

## Problem Statement

Music fans often discover artists, concert activity, and event conversation across disconnected services. Setlist Social provides a focused social experience where users can search artist data, save music interests, record personal concert activity, and watch public activity update in real time.

## Target Users

- Concertgoers who want to follow artists and events
- Music fans who want to discuss recent or upcoming shows
- Users who want lightweight social activity around setlists

## Current Scope

- React + Vite + JavaScript frontend with React Router
- Public pages for landing, about, stats, artists, activity, and discovery
- Protected pages for dashboard, profile, my concerts, wishlist, and settings
- Google OAuth / OIDC login with backend cookie authentication
- Signed-in dashboard showing user-owned counts
- User-owned My Concerts CRUD flow with authorization checks
- User-owned Wishlist flow connected to artist discovery
- Public Last.fm artist search through the backend
- Public stats, artists, and activity endpoints using database data
- SignalR public activity updates for new user-owned activity
- ASP.NET Core Minimal API backend targeting .NET 10
- Swagger/OpenAPI configured
- EF Core persistence with SQLite local development and PostgreSQL production support
- EF Core migrations and domain relationships for users, artists, concerts, reviews, wishlist items, activity events, and tags
- Development seed endpoints, including full-scale simulated seed data
- Backend integration tests, frontend Vitest tests, Playwright E2E tests, and GitHub Actions CI

## Planned Scope

- Deeper Last.fm features beyond public artist search
- More complete profile and settings workflows
- Broader protected CRUD workflows for reviews and richer setlist activity
- Additional production hardening, monitoring, and security review

## Out Of Scope For Current Version

- Secrets or environment-specific credentials
- Direct Last.fm user account/scrobble connections
- Full recommendation or personalization engine
- Administrative moderation tools
