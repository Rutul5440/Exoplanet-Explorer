---
version: 1
slug: "src-app-app-ts"
primary_target: "src/app/app.ts"
related_targets: []
---

# Surface Brief: Exoplanet Explorer Dashboard

- **Scope:** Main Operate workspace (`frontend/src/app/app.ts` and feature views)
- **Mode:** Operate (with portfolio-grade Experience clarity on interactive habitable-zone visualization)
- **Constraints:** Angular 20 standalone + Signals, Chart.js, ASP.NET Core 8 + SQL Server API (`http://localhost:5151`), WCAG AA contrast, `prefers-reduced-motion`, zero metadata leaked into rendered DOM.

## Direction contract

- **THESIS:** An orbital observatory dashboard built in the grammar of a midnight-blue enamel transit atlas—replacing generic gray SaaS cards with crisp porcelain hairline framing, four functional route inks, and an interactive habitable-zone transit corridor while keeping every filter, chart, table, and comparison control immediately familiar.
- **OWN-WORLD:** Midnight enamel navy (`#0B1230` canvas, `#101B3E` panels), porcelain white (`#FFFFFF` / `#D6E1F5`) ink and 1px hairline rules with chamfered signage corners, four functional signal inks (Emerald `#22B14C` conservative habitable zone, Amber `#FFC20E` optimistic zone, Cobalt `#1E5BFF` active transit/selection, Scarlet `#E21D2D` sync action & hot extremes), humanist signage type paired with tabular numerals.
- **STORY:** The explorer lands directly on 6,400+ synced NASA exoplanets, sees live catalog health and last sync status, slices populations via mass/radius/period/method filters or presets, inspects habitable-zone placement on the orbital corridor, reads Mass-vs-Radius and Discovery Trends charts, and pins up to 4 worlds side by side against Earth.
- **FIRST VIEWPORT:** Persistent 60px top observatory bar (title, 4 inline KPI figures, Sync NASA Archive button + sync drawer toggle) over a 280px left filter rail (search, quick presets, HZ status chips, mass/radius/period bounds, discovery method) and a main workspace stacked with: (1) the Orbital Corridor Habitable-Zone Strip for the active planet, (2) a 2-column Chart.js row (Mass vs. Radius log-log scatter + Discovery Year Trends), (3) the sortable catalog table with Pin-to-Compare buttons, and (4) the docked Side-by-Side Comparison Matrix against Earth.
- **FORM:** Midnight Transit Diagram (`wayfinding-cartography-signage-midnight-transit-diagram`, Challenger #5 chosen by user), seed:e5977b7f3d1f39ea.
- **FINISH:** unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
