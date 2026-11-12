import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ExoplanetDetail } from '../../core/models/exoplanet.model';
import { FormatMetricPipe } from '../../shared/pipes/format-metric-pipe';

interface CorridorGeometry {
  hasScale: boolean;
  maxAu: number;
  optStartPct: number;
  optEndPct: number;
  consStartPct: number;
  consEndPct: number;
  earthEqAu: number | null;
  earthEqPct: number | null;
  planetPct: number | null;
  starColor: string;
}

@Component({
  selector: 'app-habitable-corridor',
  imports: [FormatMetricPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="enamel-panel corridor" aria-label="Habitable Zone Orbital Estimator">
      @if (loading()) {
        <div class="corridor-head">
          <div class="skeleton-block" style="width: 280px; height: 22px;"></div>
          <div class="skeleton-block" style="width: 180px; height: 22px;"></div>
        </div>
        <div class="skeleton-block" style="width: 100%; height: 94px; margin-top: 12px;"></div>
      } @else if (planet(); as p) {
        <div class="corridor-head">
          <div class="planet-ident">
            <h2>{{ p.planetName }}</h2>
            <span class="host-tag">Orbiting {{ p.hostName }}</span>
            <span class="status-pill" [class]="statusClass(p.habitabilityStatus)">
              <span class="dot" aria-hidden="true"></span>
              {{ statusLabel(p.habitabilityStatus) }}
            </span>
            @if (p.isRockyCandidate) {
              <span class="status-pill conservative">Rocky Candidate (&le; 1.8 R&#8853;)</span>
            }
            @if (p.semiMajorAxisDerived) {
              <span class="derived-chip" title="Estimated from orbital period and host stellar mass via Kepler's 3rd Law">Kepler Orbit</span>
            }
            @if (p.luminosityDerived) {
              <span class="derived-chip" title="Estimated from stellar radius and effective temperature via Stefan-Boltzmann Law">Derived L&#9737;</span>
            }
          </div>

          <div class="corridor-actions">
            @if (p.earthSimilarityScore !== null) {
              <div class="esi-readout" title="Composite similarity to Earth radius and stellar insolation">
                <span class="esi-label">Earth Similarity</span>
                <span class="mono-num esi-value">{{ p.earthSimilarityScore | formatMetric: 1 }}%</span>
              </div>
            }
            <button
              type="button"
              class="btn-transit"
              [class.btn-cobalt]="isPinned()"
              (click)="pinCompare.emit(p.id)"
              [attr.aria-pressed]="isPinned()"
            >
              {{ isPinned() ? 'Pinned to Compare' : 'Pin to Compare' }}
            </button>
          </div>
        </div>

        @let g = geometry();
        @if (g.hasScale) {
          <div class="diagram-wrap">
            <svg viewBox="0 0 920 108" class="transit-svg" role="img" [attr.aria-label]="'Orbital corridor diagram for ' + p.planetName">
              <!-- Base star-to-outer corridor line -->
              <line x1="78" y1="54" x2="884" y2="54" stroke="rgba(255,255,255,0.28)" stroke-width="3" stroke-linecap="round" />

              <!-- Optimistic HZ track (Amber) -->
              <line
                [attr.x1]="pctToX(g.optStartPct)"
                y1="54"
                [attr.x2]="pctToX(g.optEndPct)"
                y2="54"
                stroke="#FFC20E"
                stroke-width="7"
                stroke-linecap="round"
                opacity="0.72"
              />

              <!-- Conservative HZ track (Emerald) -->
              <line
                [attr.x1]="pctToX(g.consStartPct)"
                y1="54"
                [attr.x2]="pctToX(g.consEndPct)"
                y2="54"
                stroke="#22B14C"
                stroke-width="11"
                stroke-linecap="round"
              />

              <!-- Conservative boundary ticks -->
              <line [attr.x1]="pctToX(g.consStartPct)" y1="40" [attr.x2]="pctToX(g.consStartPct)" y2="68" stroke="#FFFFFF" stroke-width="2" />
              <line [attr.x1]="pctToX(g.consEndPct)" y1="40" [attr.x2]="pctToX(g.consEndPct)" y2="68" stroke="#FFFFFF" stroke-width="2" />

              <!-- Earth 1.0 S_earth equivalent tick -->
              @if (g.earthEqPct !== null) {
                <line [attr.x1]="pctToX(g.earthEqPct)" y1="30" [attr.x2]="pctToX(g.earthEqPct)" y2="78" stroke="#7AA8FF" stroke-width="1.5" stroke-dasharray="3 3" />
                <text [attr.x]="pctToX(g.earthEqPct)" y="22" text-anchor="middle" fill="#93A9D2" font-size="11" font-family="JetBrains Mono, monospace">
                  1.0 S&#8853; ({{ g.earthEqAu | formatMetric: 3 }} AU)
                </text>
              }

              <!-- Host star interchange node -->
              <circle cx="52" cy="54" r="19" [attr.fill]="g.starColor" opacity="0.24" />
              <circle cx="52" cy="54" r="12" [attr.fill]="g.starColor" stroke="#FFFFFF" stroke-width="2" />
              <text x="52" y="92" text-anchor="middle" fill="#C8D6EF" font-size="11" font-family="Public Sans, sans-serif">
                {{ p.hostName }}
              </text>

              <!-- Planet station node -->
              @if (g.planetPct !== null) {
                <g [attr.transform]="'translate(' + pctToX(g.planetPct) + ', 54)'">
                  <circle r="13" fill="#0B1230" stroke="#FFFFFF" stroke-width="3" />
                  <circle r="6" [attr.fill]="planetNodeColor(p.habitabilityStatus)" />
                  <text y="34" text-anchor="middle" fill="#FFFFFF" font-size="12" font-weight="700" font-family="JetBrains Mono, monospace">
                    {{ p.semiMajorAxisAu | formatMetric: 4: 'AU' }}
                  </text>
                </g>
              }

              <!-- Scale end label -->
              <text x="884" y="92" text-anchor="end" fill="#93A9D2" font-size="11" font-family="JetBrains Mono, monospace">
                0 &rarr; {{ g.maxAu | formatMetric: 3: 'AU' }}
              </text>
            </svg>
          </div>
        } @else {
          <div class="corridor-missing">
            Insufficient host luminosity or orbital distance data to plot the Kopparapu habitable-zone corridor for {{ p.planetName }}.
          </div>
        }

        <div class="corridor-metrics">
          <div class="metric-cell">
            <span class="m-label">Conservative HZ</span>
            <span class="mono-num m-val">
              @if (p.hzInnerConservativeAu !== null && p.hzOuterConservativeAu !== null) {
                {{ p.hzInnerConservativeAu | formatMetric: 3 }} &ndash; {{ p.hzOuterConservativeAu | formatMetric: 3 }} AU
              } @else { — }
            </span>
          </div>
          <div class="metric-cell">
            <span class="m-label">Optimistic HZ</span>
            <span class="mono-num m-val">
              @if (p.hzInnerOptimisticAu !== null && p.hzOuterOptimisticAu !== null) {
                {{ p.hzInnerOptimisticAu | formatMetric: 3 }} &ndash; {{ p.hzOuterOptimisticAu | formatMetric: 3 }} AU
              } @else { — }
            </span>
          </div>
          <div class="metric-cell">
            <span class="m-label">Insolation Flux</span>
            <span class="mono-num m-val">{{ p.insolationEarth | formatMetric: 3: 'S⊕' }}</span>
          </div>
          <div class="metric-cell">
            <span class="m-label">Eq. Temperature</span>
            <span class="mono-num m-val">{{ p.equilibriumTempK | formatMetric: 0: 'K' }}</span>
          </div>
          <div class="metric-cell">
            <span class="m-label">Star Luminosity / Teff</span>
            <span class="mono-num m-val">
              {{ p.stellarLuminositySolar | formatMetric: 4: 'L☉' }} · {{ p.stellarTeffK | formatMetric: 0: 'K' }}
            </span>
          </div>
          <div class="metric-cell">
            <span class="m-label">Distance</span>
            <span class="mono-num m-val">{{ p.distanceParsecs | formatMetric: 1: 'pc' }}</span>
          </div>
        </div>
      } @else {
        <div class="corridor-missing">
          Select any planet from the Mass vs. Radius plot or catalog table to map its orbital habitable zone.
        </div>
      }
    </section>
  `,
  styles: `
    .corridor {
      padding: 16px 20px;
    }
    .corridor-head {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }
    .planet-ident {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 10px;
    }
    .planet-ident h2 {
      font-size: 24px;
      line-height: 1.1;
    }
    .host-tag {
      color: var(--ink-secondary);
      font-size: 13px;
    }
    .dot {
      width: 7px;
      height: 7px;
      border-radius: 50%;
      background: currentColor;
    }
    .derived-chip {
      font-size: 11px;
      padding: 2px 8px;
      border-radius: 4px;
      border: 1px solid var(--rule-hairline);
      color: var(--ink-secondary);
      background: rgba(255, 255, 255, 0.04);
    }
    .corridor-actions {
      display: flex;
      align-items: center;
      gap: 14px;
    }
    .esi-readout {
      display: flex;
      align-items: baseline;
      gap: 8px;
    }
    .esi-label {
      font-size: 12px;
      color: var(--ink-muted);
    }
    .esi-value {
      font-size: 18px;
      font-weight: 700;
      color: var(--route-green-text);
    }
    .diagram-wrap {
      margin: 12px 0 8px;
      background: var(--bg-input);
      border: 1px solid var(--rule-subtle);
      border-radius: var(--radius-sm);
      padding: 6px 8px;
    }
    .transit-svg {
      width: 100%;
      height: auto;
      display: block;
    }
    .corridor-metrics {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 10px 16px;
      margin-top: 10px;
      padding-top: 10px;
      border-top: 1px solid var(--rule-subtle);
    }
    .metric-cell {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .m-label {
      font-size: 12px;
      color: var(--ink-muted);
    }
    .m-val {
      font-size: 13px;
      font-weight: 600;
      color: var(--ink-primary);
    }
    .corridor-missing {
      margin-top: 10px;
      padding: 14px 16px;
      border: 1px dashed var(--rule-hairline);
      border-radius: var(--radius-sm);
      color: var(--ink-secondary);
    }
  `
})
export class HabitableCorridor {
  readonly planet = input<ExoplanetDetail | null>(null);
  readonly loading = input(false);
  readonly isPinned = input(false);
  readonly pinCompare = output<number>();

  protected readonly geometry = computed<CorridorGeometry>(() => {
    const p = this.planet();
    if (
      !p ||
      p.hzInnerOptimisticAu === null ||
      p.hzOuterOptimisticAu === null ||
      p.hzInnerConservativeAu === null ||
      p.hzOuterConservativeAu === null
    ) {
      return {
        hasScale: false,
        maxAu: 1,
        optStartPct: 0,
        optEndPct: 0,
        consStartPct: 0,
        consEndPct: 0,
        earthEqAu: null,
        earthEqPct: null,
        planetPct: null,
        starColor: '#FFC20E'
      };
    }

    const candidates = [p.hzOuterOptimisticAu * 1.25];
    if (p.semiMajorAxisAu !== null) {
      candidates.push(p.semiMajorAxisAu * 1.15);
    }
    const maxAu = Math.max(...candidates, 0.01);
    const toPct = (au: number) => Math.min(100, Math.max(0, (au / maxAu) * 100));

    const earthEqAu = p.stellarLuminositySolar && p.stellarLuminositySolar > 0
      ? Math.sqrt(p.stellarLuminositySolar)
      : null;

    return {
      hasScale: true,
      maxAu,
      optStartPct: toPct(p.hzInnerOptimisticAu),
      optEndPct: toPct(p.hzOuterOptimisticAu),
      consStartPct: toPct(p.hzInnerConservativeAu),
      consEndPct: toPct(p.hzOuterConservativeAu),
      earthEqAu,
      earthEqPct: earthEqAu !== null ? toPct(earthEqAu) : null,
      planetPct: p.semiMajorAxisAu !== null ? toPct(p.semiMajorAxisAu) : null,
      starColor: this.computeStarColor(p.stellarTeffK)
    };
  });

  protected pctToX(pct: number): number {
    return 78 + (pct / 100) * 806;
  }

  protected statusClass(status: string): string {
    return status.toLowerCase();
  }

  protected statusLabel(status: string): string {
    switch (status) {
      case 'Conservative':
        return 'Conservative HZ';
      case 'Optimistic':
        return 'Optimistic HZ';
      case 'Outside':
        return 'Outside HZ';
      default:
        return 'HZ Unknown';
    }
  }

  protected planetNodeColor(status: string): string {
    switch (status) {
      case 'Conservative':
        return '#22B14C';
      case 'Optimistic':
        return '#FFC20E';
      case 'Outside':
        return '#4E8EFF';
      default:
        return '#93A9D2';
    }
  }

  private computeStarColor(teffK: number | null): string {
    if (teffK === null) return '#FFE4A3';
    if (teffK < 3700) return '#FF7A70';
    if (teffK < 5200) return '#FFB84D';
    if (teffK < 6000) return '#FFF0B8';
    return '#A6CEFF';
  }
}

