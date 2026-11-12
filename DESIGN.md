---
name: Exoplanet Explorer
description: Midnight Transit Diagram design system for the NASA Exoplanet Observatory Dashboard.
colors:
  midnight-enamel: "#0b1230"
  enamel-rail: "#0e183c"
  enamel-panel: "#111f4a"
  enamel-elevated: "#16275a"
  enamel-input: "#080f28"
  porcelain-white: "#ffffff"
  porcelain-soft: "#c8d6ef"
  porcelain-muted: "#93a9d2"
  habitable-green: "#22b14c"
  habitable-green-text: "#5ce686"
  solar-amber: "#ffc20e"
  solar-amber-text: "#ffd24d"
  transit-cobalt: "#1e5bff"
  transit-cobalt-text: "#7aa8ff"
  signal-scarlet: "#e21d2d"
  signal-scarlet-text: "#ff8590"
typography:
  display:
    fontFamily: "Barlow Condensed, Segoe UI Variable Display, system-ui, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 700
    lineHeight: 1.1
    letterSpacing: "0.04em"
  headline:
    fontFamily: "Barlow Condensed, Segoe UI Variable Display, system-ui, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 700
    lineHeight: 1.15
    letterSpacing: "0.015em"
  body:
    fontFamily: "Public Sans, Segoe UI, system-ui, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.5
  mono:
    fontFamily: "JetBrains Mono, ui-monospace, monospace"
    fontSize: "0.8125rem"
    fontWeight: 600
    lineHeight: 1.4
rounded:
  sm: "4px"
  md: "6px"
  pill: "999px"
spacing:
  xs: "6px"
  sm: "10px"
  md: "16px"
  lg: "20px"
components:
  button-primary:
    backgroundColor: "{colors.signal-scarlet}"
    textColor: "{colors.porcelain-white}"
    padding: "0 14px"
    height: "36px"
  button-secondary:
    backgroundColor: "transparent"
    textColor: "{colors.porcelain-white}"
    rounded: "{rounded.sm}"
    padding: "0 14px"
    height: "36px"
  button-active:
    backgroundColor: "{colors.transit-cobalt}"
    textColor: "{colors.porcelain-white}"
    rounded: "{rounded.sm}"
    padding: "0 14px"
    height: "36px"
  enamel-panel:
    backgroundColor: "{colors.enamel-panel}"
    textColor: "{colors.porcelain-white}"
    rounded: "{rounded.md}"
    padding: "16px 20px"
---

# Design System: Exoplanet Explorer

## Overview

**Creative North Star: "Midnight Transit Diagram"**

Exoplanet Explorer translates the visual discipline of an enamel-fired metropolitan transit mural into a live NASA astronomical observatory dashboard. Deep midnight-blue enamel surfaces carry crisp porcelain hairline rules and four strictly functional signal inks so thousands of orbital parameters, habitable-zone corridors, and comparison rows scan instantly without ornamental clutter.

**Key Characteristics:**
- Matte nocturnal enamel surfaces (`#0B1230` to `#16275A`) layered tonally with 1px porcelain-tinted borders instead of drop shadows.
- Four semantic route inks assigned strictly to habitable-zone physics, discovery methods, active selection, and sync triggers.
- Tabular numeric alignment (`JetBrains Mono` + `tabular-nums`) isolated to physical coordinates and measurements while UI controls stay in humanist sans (`Public Sans`).

## Colors

The palette reserves high saturation exclusively for habitable-zone bands, discovery-method encoding, selection state, and sync actions over calm midnight-blue enamel grounds.

### Primary
- **Transit Cobalt** (`#1E5BFF`, readable dark-surface label `#7AA8FF`): Active filter chips, pinned toggle state, Transit discovery method series, and keyboard focus outlines (`2px solid`).
- **Signal Scarlet** (`#E21D2D`, readable text `#FF8590`): Primary `Sync NASA Archive` chamfered CTA, Radial Velocity series, selected scatter point highlight, and error alerts.

### Secondary
- **Habitable Green** (`#22B14C`, readable text `#5CE686`): Conservative Kopparapu habitable-zone track, rocky candidates (`<= 1.8 R⊕`), closest-to-Earth matrix highlights, and Imaging series.
- **Solar Amber** (`#FFC20E`, readable text `#FFD24D`): Optimistic habitable-zone outer/inner margins, Microlensing series, and running sync states.

### Neutral
- **Midnight Enamel** (`#0B1230`): Primary viewport background and SVG corridor cutout fill.
- **Enamel Rail & Panel** (`#0E183C` / `#111F4A` / `#16275A`): Sticky filter rail, analytical panels, and elevated sync log drawer.
- **Porcelain White & Soft Slate** (`#FFFFFF` / `#C8D6EF` / `#93A9D2`): Primary headings, table data, secondary labels, and 1px hairline dividers (`rgba(255, 255, 255, 0.22)`).

### Named Rules
**The Route-Ink Purity Rule.** Never use emerald, amber, cobalt, or scarlet as decorative card accents; each hue encodes a single scientific or operational meaning across charts, orbital diagrams, and tables.

## Typography

**Display Font:** `Barlow Condensed` (with `Segoe UI Variable Display`, `system-ui`, `sans-serif`)
**Body Font:** `Public Sans` (with `Segoe UI`, `system-ui`, `sans-serif`)
**Measurement Mono Font:** `JetBrains Mono` (with `ui-monospace`, `monospace`)

**Character:** Condensed transit-signage headers project institutional authority over tight, legible humanist body controls and tabular numeric readouts.

### Hierarchy
- **Display** (700, `1.5rem`, `line-height: 1.1`, `letter-spacing: 0.04em`): Uppercase `Exoplanet Explorer` observatory header.
- **Headline** (700, `1.25rem`, `line-height: 1.15`): Panel titles (`Catalog Filters`, `Mass vs. Radius`, `Confirmed Exoplanets Catalog`, `Side-by-Side World Comparison`).
- **Body** (400–600, `0.875rem`, `line-height: 1.5`): Filter controls, table rows, status notices, and empty-state guidance.
- **Measurement Mono** (600, `0.8125rem`, `tabular-nums`):AU orbital bounds, $M_\oplus$, $R_\oplus$, days, Kelvin, parsecs, and sync timestamps.

### Named Rules
**The Earned Monospace Rule.** Monospace typography is restricted strictly to numerical measurements, coordinates, and timestamps (`tabular-nums`)—never used on prose labels or navigation buttons.

## Layout

Desktop uses a sticky 60px top telemetry bar over a two-column workspace: a `280px` sticky left filter rail paired with a fluid main analytical column (`Habitable Zone Orbital Corridor` -> `2-up Charts` -> `Catalog Table` -> `Side-by-Side Comparison`). At `<= 1180px`, charts stack cleanly into one column; at `<= 960px`, the filter rail collapses behind a top-bar `Filters` toggle to prevent horizontal squeeze.

## Elevation & Depth

Surfaces remain flat at rest and communicate hierarchy strictly through stepped midnight-enamel tonal shifts (`#0B1230` -> `#0E183C` -> `#111F4A` -> `#16275A`), porcelain hairline borders, and subtle backdrop blur (`blur(10px)`) on the sticky top bar. Zero decorative drop shadows are used.

## Shapes

Panels and standard inputs use restrained architectural corners (`4px`–`6px` radius) framed by `1px` porcelain-tinted rules. The primary `Sync NASA Archive` CTA carries chamfered transit-plate geometry (`polygon(8px 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%, 0 8px)`), while habitability badges and route indicators use clean circular/pill geometry.

## Components

### Buttons
- **Primary (`Sync NASA Archive`):** Signal scarlet (`#E21D2D`), white ink, chamfered corners, `36px` minimum height, `14px` horizontal padding.
- **Secondary / Ghost:** Transparent over enamel with `1px` porcelain hairline border (`rgba(255,255,255,0.22)`), brightening border and fill on hover.
- **Active Selection (`btn-cobalt` / `.chip.active`):** Transit cobalt (`#1E5BFF`) fill with `#FFFFFF` ink.

### Inputs & Filter Controls
- **Style:** Deep enamel input field (`#080F28`), `1px` hairline border, `4px` radius, `36px` minimum height.
- **Focus:** `2px solid #7AA8FF` outline with `2px` offset.

### Orbital Corridor Diagram (Signature Component)
- **Structure:** Responsive SVG track (`viewBox="0 0 920 108"`) anchored by a star node tinted by stellar effective temperature (`Teff`), overlaid with Solar Amber optimistic bounds, Habitable Green conservative bounds, a dashed `1.0 S⊕` tick, and a ringed porcelain station node marking the selected exoplanet's semi-major axis.

## Do's and Don'ts

### Do:
- **Do** keep numeric table columns right-aligned with `.mono-num` (`tabular-nums`) and show `—` for missing NASA measurements.
- **Do** disclose derived physics fields (`Kepler Orbit`, `Derived L☉`) and sampled scatter counts (`Showing X of Y`) transparently.
- **Do** respect `prefers-reduced-motion: reduce` across all transitions and skeleton shimmers.

### Don't:
- **Don't** add eyebrow kickers above section headings, gradient text, decorative grid backgrounds, or thick colored `border-left` stripes on panels.
- **Don't** nest bordered cards inside cards or hide core catalog controls behind unnecessary modal dialogs.

