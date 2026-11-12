# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

Angular 20 (standalone + Signals + CSS) · Chart.js · ASP.NET Core 8 (`net8.0`) · SQL Server 2022 (`localhost`, Windows Auth) · NASA Exoplanet Archive TAP API (`pscomppars`)

## Users

- **Primary user:** Astronomy enthusiasts, students, data explorers, and engineering/portfolio reviewers inspecting full-stack data engineering and scientific visualization.
- **Situation & job:** Browsing and filtering 6,400+ confirmed NASA exoplanets, evaluating habitable-zone candidacy from stellar luminosity and orbital semi-major axis, spotting catalog-wide discovery trends, and comparing worlds side by side against Earth.

## Product Purpose

Provide a fast, scientifically transparent NASA exoplanet observatory dashboard backed by an automated nightly SQL Server catalog sync, cached analytical queries, interactive Mass-vs-Radius and Discovery-Year charts, Kopparapu habitable-zone estimation, and side-by-side world comparison.

## Positioning

Combines live NASA Exoplanet Archive ingestion and transparent Kopparapu habitable-zone physics (handling logarithmic `st_lum` values, derived Kepler orbits, and rocky radius cutoffs) with a responsive, task-first observatory interface and Earth-baseline comparison.

## Operating Context

- Data originates from NASA's public `pscomppars` TAP feed and refreshes automatically via a nightly background service (`02:00 UTC`), with an explicit on-demand sync trigger and sync history telemetry for demonstration and maintenance.
- Users operate interactively across filter ranges (mass, radius, orbital period, discovery method, habitable-zone status, search), linked scatter/trend charts, an orbital habitable-zone visualizer, and a comparison dock (up to 4 exoplanets + Earth).

## Capabilities and Constraints

- Nightly background synchronization with overlap lock and hash-based incremental SQL Server upserts (`Exoplanets` + `SyncRuns`).
- Server-side filtering, deterministic sorting, pagination, `IMemoryCache` epoch invalidation on sync, and input validation.
- Interactive Chart.js visualizations (Mass vs. Radius log-log scatter, Discovery Year Trends stacked by method), interactive habitable-zone orbital strip, and side-by-side comparison against Earth.
- Habitable-zone classification distinguishes `Conservative`, `Optimistic`, `Outside`, and `Unknown`, plus a rocky-planet flag (`<= 1.8 R_earth`) so gas giants inside the habitable zone are clearly identified.
- *Inferred:* English single-locale interface; unauthenticated public/portfolio dashboard without multi-user accounts.

## Evidence on Hand

- Live NASA Exoplanet Archive TAP `pscomppars` endpoint verified (`6,464` confirmed exoplanet records, exact field schema verified).
- ASP.NET Core 8 backend (`backend/ExoplanetExplorer.Api`) and 12 passing xUnit unit/integration tests (`backend/ExoplanetExplorer.Tests`) verified.

## Product Principles

1. **Scientific honesty first:** Show units, derived-field indicators (when orbit or luminosity used fallback physics), and Earth baselines clearly—never round away uncertainty or conflate gas giants with rocky worlds.
2. **Instant exploration at catalog scale:** Server-side filtering and in-memory cached chart payloads keep interactions crisp across 6,400+ records.
3. **Linked multi-view discovery:** Selecting a world in the scatter plot or catalog table immediately updates the habitable-zone orbital view and allows one-click pinning to compare.
4. **Operational transparency:** Sync state, last NASA refresh timestamp, and partial-sample counts are visible without cluttering exploration.

## Accessibility & Inclusion

- WCAG AA contrast (≥4.5:1 body, ≥3:1 large text), full keyboard navigation across filters, charts/table selection, comparison dock, and `prefers-reduced-motion` support.

