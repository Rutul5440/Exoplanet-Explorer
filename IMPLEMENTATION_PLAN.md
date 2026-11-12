# Exoplanet Explorer — Implementation & Global Skills Plan

> **Project:** Exoplanet Explorer: NASA Data Dashboard  
> **Stack:** Angular 20 · ASP.NET Core 8 (`net8.0`) · SQL Server · NASA Exoplanet Archive TAP API · Chart.js  
> **Goal:** A portfolio-grade observatory dashboard to browse 6,400+ confirmed exoplanets, filter by physical and orbital properties, visualize mass-vs-radius and discovery trends, evaluate habitable-zone candidacy from stellar luminosity and orbit, and compare planets side by side.

---

## 1. Global Skills Inventory & Usage Map ("Which Skill Is Perfect For What")

Before writing application code, this section maps **every global skill available on this system** to the exact backend, frontend, and verification tasks where it delivers the highest value.

### 1.1 Backend Skills (ASP.NET Core 8 · SQL Server · NASA API Sync · Caching · Tests)

Currently installed global backend-applicable skills come from the **Ponytail engineering & quality suite** (`C:\Users\rutul\.gemini\config\skills\`):

| Skill | Location | Perfect For (Backend Scope) | How We Will Use It |
| :--- | :--- | :--- | :--- |
| **`ponytail`** | `~/.gemini/config/skills/ponytail/SKILL.md` | **Lean architecture, API design, sync service, caching, habitable-zone math, boundary validation, and unit checks** | Enforces the smallest complete ASP.NET Core 8 implementation—no speculative MediatR/CQRS/Repository boilerplate over EF Core `DbContext`. Keeps validation strict at API & NASA TAP trust boundaries, protects data writes transactionally, leaves `shortcut: <limit>, <upgrade>` comments for any bounded shortcut, and mandates targeted tests for every non-trivial module (habitable-zone physics, TAP parser, batch upsert, cache invalidation). |
| **`ponytail-review`** | `~/.gemini/config/skills/ponytail-review/SKILL.md` | **Per-phase code, security & concurrency review** | Invoked after building each backend slice (Data/EF Core, NASA Sync `BackgroundService`, Habitable-Zone Engine, Query/Filter & Cache APIs). Checks for SQL injection/EF parameterization, concurrent sync races (`SemaphoreSlim` lock between nightly `PeriodicTimer` and manual trigger), null/negative `st_lum` edge cases, cache-busting after sync, and dead/duplicate code. |
| **`ponytail-audit`** | `~/.gemini/config/skills/ponytail-audit/SKILL.md` | **Whole-repo architecture, load, security & lean audit** | Invoked once backend + database + frontend integration are complete. Traces every flow end-to-end (`NASA TAP API -> BackgroundService -> SQL Server -> MemoryCache/OutputCache -> Angular`) under expected load (6,464+ rows, concurrent dashboard reads) and reports any must-fix bugs or removable bloat. |
| **`ponytail-debt`** | `~/.gemini/config/skills/ponytail-debt/SKILL.md` | **Deferred-limit & shortcut tracking** | Invoked before handoff to scan all `// shortcut:` comments across C#, SQL, and TypeScript into a transparent ledger with ceilings and upgrade triggers. |

> **Installed Backend Framework & EF Core Skills (`~/.agents/skills/`):**  
> Per user confirmation via **`find-skills`**, the following specialized global skills are now installed and active alongside the `ponytail` suite:
> - **`dotnet-backend-patterns`** (`~/.agents/skills/dotnet-backend-patterns/SKILL.md`) — ASP.NET Core 8 Web APIs, `IOptions<T>`, `IHttpClientFactory`, `IMemoryCache`, `CancellationToken`, DTOs, xUnit + `WebApplicationFactory` tests
> - **`dotnet-best-practices`** (`~/.agents/skills/dotnet-best-practices/SKILL.md`) — Primary constructors, XML doc comments, structured `ILogger`, input validation, secure parameterized queries
> - **`ef-core`** (`~/.agents/skills/ef-core/SKILL.md`) — `IEntityTypeConfiguration<T>`, `AsNoTracking()`, `Select` projections, batched `SaveChangesAsync`, transactional writes, SQLite integration testing
> - **`optimizing-ef-core-queries`** (`~/.agents/skills/optimizing-ef-core-queries/SKILL.md`) — SQL-pushed `IQueryable` filtering/aggregation, workload-matched indexes, bounded change tracking during batch upserts, non-overlapping `DbContext` usage
> - **`angular-developer` & `angular-new-app`** (`~/.agents/skills/angular-developer/SKILL.md`, `angular-new-app/SKILL.md`) — Official Angular team standalone components, Signals, and v20+ architecture
>
> **Confirmed Stack Targets:** SQL Server 2022 on `localhost` (Windows Auth) · Direct `chart.js` in Angular 20.

---

### 1.2 Frontend & UI/UX Skills (Angular 20 · Chart.js · Portfolio Polish)

| Skill | Location | Perfect For (Frontend Scope) | How We Will Use It |
| :--- | :--- | :--- | :--- |
| **`impeccable`** | `~/.gemini/config/skills/impeccable/SKILL.md` | **Primary UI architecture, design tokens (`PRODUCT.md` / `DESIGN.md`), accessibility & craft floor** | Runs `init` (`PRODUCT.md`) and `shape`/`new-work` (Operate + Experience hybrid direction contract, `DESIGN.md` at finish). Enforces `operate.md` and `craft-floor.md`: crisp data-table scanability, `tabular-nums`, accessible contrast (≥4.5:1), skeleton loading states, real empty/error recovery copy, keyboard navigation, and zero generic template slop (no kickers/eyebrows, no nested cards). Closes with `audit`, `polish`, and `harden`. |
| **`frontend-design`** | `~/.agents/skills/frontend-design/SKILL.md` | **Visual identity, palette/type token plan & plain-language UX copy** | Shapes a distinctive astronomical-observatory aesthetic (4–6 named hex tokens, deliberate font pairing, ASCII wireframe layout review before coding) and enforces clear user-facing copy (e.g., *"NASA Archive Sync"* and *"Last synced"* instead of backend jargon). |
| **`antigravity-design-expert`** | `~/.agents/skills/antigravity-design-expert/SKILL.md` | **Spatial depth, glassmorphic overlays & GPU-smooth state motion** | Applied selectively for layered elevation, subtle frosted comparison/inspector surfaces (`backdrop-filter`), and reduced-motion accessibility (`prefers-reduced-motion: reduce`, `will-change: transform`). *Stack override:* its default React/Next/GSAP preference is explicitly overridden by the Angular 20 brief; we translate its spatial/motion principles into pure Angular + CSS. |
| **`ponytail` / `ponytail-review`** | `~/.gemini/config/skills/ponytail*/SKILL.md` | **Lean Angular components, signal state & Chart.js wrapper** | Prevents bloated Angular service/NgRx boilerplate; uses lightweight Angular Signals + `HttpClient` + a lean standalone Chart.js directive rather than heavy multi-layer state libraries. |

---

### 1.3 Discovery & Platform Utility Skills (Reference)

| Skill | Location | Role in This Project |
| :--- | :--- | :--- |
| **`find-skills`** | `~/.agents/skills/find-skills/SKILL.md` | Used to discover optional `.NET`, `EF Core`, and `Angular` skills (`npx skills find`); installs only with explicit user approval. |
| **`ponytail-gain` / `ponytail-help`** | `~/.gemini/config/skills/ponytail-*/SKILL.md` | Read-only reference/benchmark tools (not part of build pipeline). |
| **Built-in Antigravity Skills** (`native_swarm`, `generative_ui`, `automation`, etc.) | `~/.gemini/antigravity/builtin/skills/` | Optional workflow helpers (`native_swarm` only if parallelizing backend and frontend implementation later; not required for single-agent build). |

---

## 2. Verified Local Environment & External API Facts

Checked directly on this workstation prior to planning:

- **.NET SDK & Runtime:** `.NET SDK 9.0.305` installed alongside `Microsoft.AspNetCore.App 8.0.20` and `Microsoft.NETCore.App 8.0.20 / 8.0.27 / 8.0.34` runtimes. Targeting `<TargetFramework>net8.0</TargetFramework>` compiles and runs natively on ASP.NET Core 8.
- **SQL Server:** Both `SQL Server 2022` (`localhost` via Windows Auth) and `SQL Server 2019 LocalDB` (`(localdb)\MSSQLLocalDB`) are running and reachable.
- **Node & Angular CLI:** `Node v22.16.0`, `npm 11.3.0`, `@angular/cli 20.3.9`.
- **NASA Exoplanet Archive TAP API (Live Verified):**
  - Endpoint: `https://exoplanetarchive.ipac.caltech.edu/TAP/sync`
  - Table: `pscomppars` (Planetary Systems Composite Parameters — currently **6,464 confirmed exoplanets** with composite stellar & planetary parameters, reducing null gaps compared to raw single-paper rows).
  - Verified column schema & units:
    - `pl_name` (Planet Name), `hostname` (Host Star Name)
    - `discoverymethod` (Discovery Method), `disc_year` (Discovery Year)
    - `pl_rade` (Planet Radius, Earth Radii), `pl_bmasse` (Planet Mass or $M\sin i$, Earth Masses)
    - `pl_orbper` (Orbital Period, days), `pl_orbsmax` (Semi-Major Axis, AU), `pl_eqt` (Equilibrium Temperature, K)
    - `st_lum` (Stellar Luminosity in **`log(Solar)` = $\log_{10}(L / L_\odot)$** — *critical*: values are logarithmic and often negative for M/K dwarfs, so linear luminosity must be computed via $10^{\text{st\_lum}}$!)
    - `st_teff` (Effective Temp, K), `st_rad` (Solar Radii), `st_mass` (Solar Masses), `sy_dist` (Distance, parsecs)
  - *Note:* `rowupdate` does not exist on `pscomppars`, so nightly sync fetches the composite catalog cleanly in one compressed JSON stream and upserts changed/new rows by `pl_name`.

---

## 3. System Architecture (`ponytail`-Lean Design)

```
d:\Projects\Exoplanet Explorer\
├── IMPLEMENTATION_PLAN.md
├── PRODUCT.md                        # Created via impeccable `init`
├── DESIGN.md                         # Created at UI finish via impeccable
├── backend/
│   ├── ExoplanetExplorer.sln
│   ├── ExoplanetExplorer.Api/
│   │   ├── Program.cs                # DI, EF Core, MemoryCache, OutputCache, CORS, BackgroundService, Endpoints
│   │   ├── Data/
│   │   │   ├── ExoplanetDbContext.cs # SQL Server EF Core 8 context + indexes
│   │   │   ├── Entities/
│   │   │   │   ├── Exoplanet.cs
│   │   │   │   └── SyncRun.cs
│   │   │   └── Migrations/
│   │   ├── Models/
│   │   │   ├── ExoplanetDtos.cs      # FilterQuery, PagedResult, ComparisonDto, ChartDataDto, SyncStatusDto
│   │   │   └── NasaTapRow.cs         # Typed TAP JSON projection
│   │   ├── Services/
│   │   │   ├── HabitableZoneCalculator.cs  # Pure deterministic physics/HZ model
│   │   │   ├── NasaTapClient.cs            # Typed HttpClient w/ timeout & retry
│   │   │   ├── ExoplanetSyncService.cs     # Idempotent batch upsert + cache invalidation
│   │   │   ├── NightlySyncWorker.cs        # BackgroundService (PeriodicTimer + startup seed check)
│   │   │   └── ExoplanetQueryService.cs    # Filtered queries, stats, chart aggregations + IMemoryCache
│   │   └── Endpoints/
│   │       ├── ExoplanetEndpoints.cs
│   │       └── SyncEndpoints.cs
│   └── ExoplanetExplorer.Tests/
│       ├── HabitableZoneCalculatorTests.cs
│       ├── NasaTapParsingTests.cs
│       ├── ExoplanetSyncServiceTests.cs
│       └── ExoplanetQueryFilterTests.cs
└── frontend/                         # Angular 20 standalone + Signals + Chart.js
    └── src/app/
        ├── core/
        │   ├── models/exoplanet.models.ts
        │   └── services/exoplanet-api.service.ts
        ├── features/
        │   ├── dashboard/            # Main observatory shell, telemetry strip, presets
        │   ├── filters/              # Mass, radius, orbital period, method, HZ status, search
        │   ├── charts/               # Mass-vs-Radius log-log scatter & Discovery Year trend chart
        │   ├── habitability/         # Interactive HZ orbital ring/strip visualizer + score breakdown
        │   ├── catalog-table/        # Sortable paginated table + quick-pin for compare
        │   └── compare-drawer/       # Side-by-side 2–4 planet + Earth reference comparison
        └── shared/                   # Chart.js directive, skeleton loaders, unit/format pipes
```

**Why this structure?** (`ponytail` check):
- Two .NET projects (`Api` + `Tests`) instead of 5+ premature Clean Architecture projects (`Domain`/`Application`/`Infrastructure`/`Persistence`). Every domain rule lives cleanly in single-purpose services with zero pass-through wrappers.
- Charting uses **`chart.js` directly** behind one ~40-line standalone Angular canvas directive rather than adding wrapper-library version lag (`ng2-charts`/`ngx-charts` Angular 20 compatibility friction), giving exact control over log-log axes, habitable-zone highlight series, and click-to-inspect point events.

---

## 4. Database Schema (SQL Server)

### Table: `Exoplanets`
- `Id` (`int`, PK identity)
- `PlanetName` (`nvarchar(120)`, unique index `UX_Exoplanets_PlanetName` — natural key `pl_name`)
- `HostName` (`nvarchar(100)`, indexed)
- `DiscoveryMethod` (`nvarchar(64)`, indexed)
- `DiscoveryYear` (`int?`, indexed)
- `MassEarth` (`double?`, indexed — `pl_bmasse`)
- `RadiusEarth` (`double?`, indexed — `pl_rade`)
- `OrbitalPeriodDays` (`double?`, indexed — `pl_orbper`)
- `SemiMajorAxisAu` (`double?` — `pl_orbsmax`, or derived from Kepler's 3rd law when null and `st_mass` + `pl_orbper` exist)
- `SemiMajorAxisDerived` (`bit` — transparency flag when orbit AU was computed from period & stellar mass)
- `EquilibriumTempK` (`double?` — `pl_eqt`)
- `StellarLuminosityLogSolar` (`double?` — raw `st_lum` in dex)
- `StellarLuminositySolar` (`double?` — computed $10^{\text{st\_lum}}$, or Stefan-Boltzmann fallback from `st_teff` & `st_rad`)
- `StellarTeffK` (`double?`), `StellarRadiusSolar` (`double?`), `StellarMassSolar` (`double?`), `DistanceParsecs` (`double?`)
- **Computed Habitable-Zone Fields (persisted at sync for fast SQL filtering & sorting):**
  - `HzInnerConservativeAu` (`double?`), `HzOuterConservativeAu` (`double?`)
  - `HzInnerOptimisticAu` (`double?`), `HzOuterOptimisticAu` (`double?`)
  - `InsolationEarth` (`double?` — $S / S_\oplus = L_\star / a^2$)
  - `HabitabilityStatus` (`nvarchar(32)`, indexed — `Conservative`, `Optimistic`, `Outside`, `Unknown`)
  - `IsRockyCandidate` (`bit` — true when `RadiusEarth` is in `(0, 1.8]`)
  - `EarthSimilarityScore` (`double?` — transparent 0–100 composite similarity index when radius & flux/temp exist)
- `LastSyncedUtc` (`datetime2`), `ContentHash` (`varchar(64)` — skip unchanged rows during nightly upsert)

### Table: `SyncRuns`
- `Id` (`int`, PK identity)
- `StartedUtc` (`datetime2`), `CompletedUtc` (`datetime2?`)
- `TriggerType` (`nvarchar(16)` — `Startup`, `Nightly`, `Manual`)
- `Status` (`nvarchar(16)` — `Running`, `Succeeded`, `Failed`, `SkippedBusy`)
- `FetchedCount` (`int`), `InsertedCount` (`int`), `UpdatedCount` (`int`), `UnchangedCount` (`int`)
- `ErrorMessage` (`nvarchar(1000)?`)

---

## 5. Habitable-Zone Scientific Engine

Implemented in `HabitableZoneCalculator.cs` and unit-tested thoroughly in `HabitableZoneCalculatorTests.cs`:

1. **Stellar Luminosity ($L_\star / L_\odot$):**
   - Primary: `L = Math.Pow(10.0, st_lum)` (since NASA `st_lum` is $\log_{10}(L/L_\odot)$).
   - Fallback (when `st_lum` is null, `st_teff > 0`, `st_rad > 0`): Stefan-Boltzmann law $L/L_\odot = (R_\star/R_\odot)^2 \cdot (T_{\text{eff}} / 5772)^4$.
2. **Orbital Semi-Major Axis ($a$ in AU):**
   - Primary: `pl_orbsmax`.
   - Fallback (when `pl_orbsmax` is null, `pl_orbper > 0`, `st_mass > 0`): Kepler's 3rd Law $a = (M_\star \cdot (P_{\text{days}} / 365.25)^2)^{1/3}$ (`SemiMajorAxisDerived = true`).
3. **Kopparapu et al. (2013/2014) Habitable-Zone Flux Boundaries:**
   - Uses $T_* = \text{clamp}(T_{\text{eff}}, 2600, 7200) - 5780\text{ K}$ (defaulting to Solar `5780 K` if `st_teff` is null, flagged cleanly) and polynomial $S_{\text{eff}}(T_*)$ coefficients for:
     - **Optimistic Inner (Recent Venus)** & **Conservative Inner (Runaway Greenhouse)**
     - **Conservative Outer (Maximum Greenhouse)** & **Optimistic Outer (Early Mars)**
   - Distance bounds: $d_{\text{AU}} = \sqrt{L_\star / S_{\text{eff}}}$.
   - **Classification:**
     - `Conservative` if `HzInnerConservativeAu <= a <= HzOuterConservativeAu`
     - `Optimistic` if within optimistic bounds outside conservative bounds
     - `Outside` if `L` and `a` are known and outside optimistic bounds
     - `Unknown` if luminosity or semi-major axis cannot be resolved
   - **Rocky Candidate Flag:** `RadiusEarth <= 1.8` (Earth-to-super-Earth boundary) highlighted separately in filters and UI so gas giants inside the habitable zone (e.g., warm Jupiters) are clearly distinguished from rocky Earth-like worlds!

---

## 6. Backend API, Caching & Nightly Background Sync

### Endpoints (`/api`)
- `GET /api/exoplanets` — Paginated, sortable list with filters: `search`, `minMass`, `maxMass`, `minRadius`, `maxRadius`, `minPeriod`, `maxPeriod`, `discoveryMethod`, `habitabilityStatus`, `rockyOnly`, `sortBy`, `sortDir`, `page`, `pageSize` (clamped `1..100`).
- `GET /api/exoplanets/{id}` — Full planet + stellar + habitable-zone breakdown (with Earth reference values).
- `GET /api/exoplanets/compare?ids=1,2,3` — Side-by-side comparison payload for 1–4 planets + Earth baseline.
- `GET /api/charts/mass-radius` — Aggregated/sampled scatter dataset (filter-aware, up to 2,500 points + Earth/Solar reference markers + HZ candidates overlay) cached in `IMemoryCache`.
- `GET /api/charts/discovery-trends` — Discovery count per year grouped by `DiscoveryMethod`, cached in `IMemoryCache`.
- `GET /api/stats/summary` — KPI summary (total confirmed planets, conservative/optimistic HZ count, rocky HZ count, distinct methods, latest sync timestamp).
- `GET /api/sync/status` — Latest `SyncRun` record + recent sync history (last 10 runs).
- `POST /api/sync/trigger` — Triggers an on-demand sync (returns `202 Accepted` or `409 Conflict` if a sync is already running) for portfolio/demo verification without waiting until midnight.

### Nightly Sync & Cache Invalidation
- **`NightlySyncWorker : BackgroundService`:**
  - On startup: checks if `Exoplanets` table is empty (or older than 24h) and runs an initial seed sync in the background without blocking Kestrel startup.
  - Nightly schedule: uses `PeriodicTimer` aligned to `02:00 UTC` (configurable via `SyncOptions:DailyRunHourUtc`).
  - Concurrency guard: `SemaphoreSlim(1, 1)` ensures startup, nightly, and manual triggers never overlap.
  - Streaming & transactional batch upsert: reads TAP JSON stream, hashes each normalized record (`ContentHash`), inserts new rows and updates only changed rows inside EF Core transactions, logs exact counts to `SyncRuns`.
  - **Cache invalidation:** `ExoplanetQueryService` tracks a `CancellationTokenSource` / cache epoch key in `IMemoryCache`; whenever `ExoplanetSyncService` commits new/updated rows, it rotates the cache token so all filtered queries, summary KPIs, and chart aggregations refresh immediately on the next request.

---

## 7. Frontend Features & UI/UX Plan

1. **Observatory Overview & Sync Telemetry Bar**
   - Shows confirmed catalog total, rocky habitable-zone candidates, active filter count, and last NASA Archive sync status + manual "Sync NASA Data" action.
2. **Multi-Facet Filter Rail & Quick Presets**
   - Range inputs/sliders for Mass ($M_\oplus$), Radius ($R_\oplus$), Orbital Period (days), Discovery Method multi/select dropdown, Habitable-Zone filter (`All`, `Conservative HZ`, `Optimistic HZ`, `Rocky HZ Candidates`), and instant search by planet or host star.
   - One-click portfolio presets: *Potentially Habitable Rocky Worlds*, *TRAPPIST-1 System*, *Ultra-Short Period Lava Worlds*, *Gas Giants*.
3. **Interactive Data Visualizations (Chart.js)**
   - **Mass vs. Radius Scatter Chart (Log–Log):** Color-coded by habitability status with reference overlays (Earth, Neptune, Jupiter) and hover/click inspection to add to compare or open detail drawer.
   - **Discovery Year Trends Chart:** Stacked yearly timeline by discovery method (Radial Velocity, Transit, Microlensing, Direct Imaging, etc.) highlighting telescope eras.
4. **Habitable-Zone Orbital Estimator**
   - Visualizes any selected planet's orbital semi-major axis relative to its host star's computed Conservative & Optimistic Habitable Zone (in AU), alongside insolation flux ($S/S_\oplus$) and equilibrium temperature compared to Earth.
5. **Side-by-Side Planet Comparison Dock**
   - Pin up to 4 planets from the table or scatter chart and compare them in a dedicated side-by-side matrix alongside Earth as a baseline, highlighting best/closest-to-Earth metrics.

---

## 8. Execution Phases & Skill Gates

| Phase | Deliverables | Active Global Skills | Exit Gate |
| :--- | :--- | :--- | :--- |
| **Phase 0: Skill & Environment Alignment** | Confirm this plan; optionally install recommended `.NET`/`EF Core`/`Angular` skills via `find-skills`; verify SQL Server DB creation | `find-skills` (if approved) | User approves plan & DB target |
| **Phase 1: Backend Core & SQL Schema** | `ExoplanetExplorer.sln`, EF Core 8 models, SQL Server migrations, `HabitableZoneCalculator` + unit tests | `ponytail`, `ponytail-review` | `dotnet test` green on HZ physics & edge cases |
| **Phase 2: NASA TAP Integration & Nightly Worker** | `NasaTapClient`, `ExoplanetSyncService` (idempotent batch upsert), `NightlySyncWorker`, sync tests | `ponytail`, `ponytail-review` | Live NASA sync populates 6,464 rows cleanly in SQL Server; re-sync logs 0 unnecessary updates |
| **Phase 3: Query, Chart & Caching APIs** | Filter/sort/pagination endpoints, `compare`, `mass-radius`, `discovery-trends`, `IMemoryCache` + epoch invalidation, API tests | `ponytail`, `ponytail-review` | All API endpoints + cache invalidation verified via `dotnet test` & live HTTP checks |
| **Phase 4: Frontend Product & Design Framing** | `PRODUCT.md`, surface direction brief, design token system & layout wireframe | `impeccable` (`init`, `shape`/`new-work`), `frontend-design`, `antigravity-design-expert` | Confirmed UI structure & visual tokens |
| **Phase 5: Angular 20 Dashboard Build** | Standalone Angular app, signals state, filter rail, Chart.js scatter + trend charts, HZ orbital visualizer, comparison matrix, detail drawer | `impeccable` (`craft-floor`, `operate`), `frontend-design`, `antigravity-design-expert`, `ponytail` | `ng build` passes with zero errors; live UI flows verified against ASP.NET Core 8 API |
| **Phase 6: Whole-Repo Audit, Polish & Debt Ledger** | End-to-end verification, responsive & accessibility pass, `DESIGN.md` capture, shortcut ledger | `ponytail-audit`, `ponytail-debt`, `impeccable` (`audit`, `polish`, `harden`, `document`) | All checks pass; zero critical audit findings |
