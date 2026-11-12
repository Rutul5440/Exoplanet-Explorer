# Exoplanet Explorer: NASA Data Dashboard

An interactive observatory dashboard for exploring confirmed NASA exoplanets, analyzing mass-vs-radius populations and discovery trends, evaluating Kopparapu habitable-zone candidacy, and comparing worlds side by side against Earth.

## Stack

- **Frontend:** Angular 20 (standalone components, Signals, `ChangeDetectionStrategy.OnPush`), Chart.js
- **Backend:** ASP.NET Core 8 (`net8.0`) Minimal APIs, EF Core 8, `IMemoryCache`, `BackgroundService`
- **Database:** SQL Server 2022 (`localhost`, Windows Auth)
- **Data Source:** NASA Exoplanet Archive TAP API (`pscomppars`)

## Key Features

- **Nightly NASA Sync Worker:** `NightlySyncWorker` seeds an empty catalog on startup and runs daily at `02:00 UTC` (plus an on-demand `POST /api/sync/trigger` endpoint), using a `SemaphoreSlim` overlap lock and SHA-256 `ContentHash` batch upserts inside SQL transactions.
- **Habitable-Zone Physics Engine:** `HabitableZoneCalculator` converts NASA's logarithmic stellar luminosity (`st_lum` in $\log_{10}(L/L_\odot)$) to solar luminosity (falling back to Stefan-Boltzmann when absent and Kepler's 3rd Law when semi-major axis is missing) and classifies each world against Kopparapu et al. conservative/optimistic boundaries alongside a rocky-planet flag ($\le 1.8\,R_\oplus$) and Earth Similarity Index.
- **Cached Analytical Queries:** `ExoplanetQueryService` pushes filtering, deterministic sorting, pagination, and aggregations to SQL Server and caches results in a bounded `IMemoryCache` invalidated via an atomic epoch counter whenever a sync commits changes.
- **Interactive Visualizations & Comparison:** Log–log Mass vs. Radius scatter plot with Solar baselines, stacked Discovery Year Trends by method, an interactive Midnight Transit Orbital Corridor diagram, and a 4-world side-by-side comparison matrix against Earth.

## Running Locally

### 1. Apply the SQL Server Migration

Review [InitialCreate](backend/ExoplanetExplorer.Api/Data/Migrations/20261111175332_InitialCreate.cs) and apply it to create the `ExoplanetExplorer` database on `localhost`:

```powershell
cd "d:\Projects\Exoplanet Explorer\backend\ExoplanetExplorer.Api"
dotnet ef database update
```

### 2. Start the ASP.NET Core 8 API (`http://localhost:5151`)

```powershell
cd "d:\Projects\Exoplanet Explorer\backend\ExoplanetExplorer.Api"
dotnet run
```

On first launch against an empty database, `NightlySyncWorker` automatically seeds confirmed exoplanets from NASA's `pscomppars` TAP feed.

### 3. Start the Angular 20 Dashboard (`http://localhost:4200`)

```powershell
cd "d:\Projects\Exoplanet Explorer\frontend"
npm start
```

The Angular dev proxy ([proxy.conf.json](frontend/proxy.conf.json)) forwards `/api` requests to `http://localhost:5151`.

## Verification

- **Backend tests:** `cd backend && dotnet test` (xUnit unit tests + SQLite-backed `WebApplicationFactory` integration tests)
- **Frontend build:** `cd frontend && npx ng build`

