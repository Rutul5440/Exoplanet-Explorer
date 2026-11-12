import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ComparisonResponse, ExoplanetDetail } from '../../core/models/exoplanet.model';
import { FormatMetricPipe } from '../../shared/pipes/format-metric-pipe';

interface CompareRowSpec {
  label: string;
  unit: string;
  read: (p: ExoplanetDetail) => number | null;
  decimals: number;
}

@Component({
  selector: 'app-compare-panel',
  imports: [FormatMetricPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="enamel-panel compare-section" aria-label="Side-by-Side Planet Comparison">
      <header class="compare-head">
        <div>
          <h2>Side-by-Side World Comparison</h2>
          <p class="compare-sub">
            Compare up to 4 pinned exoplanets directly against Earth (1.0 baseline)
          </p>
        </div>

        <div class="compare-actions">
          @if (pinnedIds().length > 0) {
            <button type="button" class="btn-transit" (click)="clearPinned.emit()">Clear All</button>
          } @else {
            <button type="button" class="btn-transit btn-cobalt" (click)="loadSuggested.emit()">
              Compare Top Habitable Pair
            </button>
          }
        </div>
      </header>

      @if (loading()) {
        <div class="skeleton-block" style="height: 220px; width: 100%; margin-top: 12px;"></div>
      } @else if (comparison(); as cmp) {
        <div class="matrix-scroll">
          <table>
            <thead>
              <tr>
                <th class="metric-col">Property</th>
                <th class="earth-col">
                  <div class="col-title">Earth</div>
                  <div class="col-sub">Solar Baseline</div>
                </th>
                @for (p of cmp.planets; track p.id) {
                  <th>
                    <div class="col-head-flex">
                      <div>
                        <div class="col-title">{{ p.planetName }}</div>
                        <div class="col-sub">{{ p.hostName }} &middot; {{ p.discoveryMethod }}</div>
                      </div>
                      <button
                        type="button"
                        class="unpin-btn"
                        [attr.aria-label]="'Remove ' + p.planetName + ' from comparison'"
                        (click)="unpinPlanet.emit(p.id)"
                      >
                        &times;
                      </button>
                    </div>
                  </th>
                }
              </tr>
            </thead>
            <tbody>
              <tr>
                <td class="metric-col">Habitability Verdict</td>
                <td class="earth-col"><span class="status-pill conservative">Conservative</span></td>
                @for (p of cmp.planets; track p.id) {
                  <td>
                    <span class="status-pill" [class]="p.habitabilityStatus.toLowerCase()">
                      {{ p.habitabilityStatus }}
                    </span>
                    @if (p.isRockyCandidate) {
                      <span class="rocky-note">Rocky</span>
                    }
                  </td>
                }
              </tr>
              @for (spec of metricRows; track spec.label) {
                <tr>
                  <td class="metric-col">
                    {{ spec.label }}
                    @if (spec.unit) {
                      <span class="unit-tag">({{ spec.unit }})</span>
                    }
                  </td>
                  <td class="mono-num earth-col">{{ spec.read(cmp.earthReference) | formatMetric: spec.decimals }}</td>
                  @let closestId = findClosestPlanetId(cmp, spec);
                  @for (p of cmp.planets; track p.id) {
                    <td class="mono-num" [class.closest-earth]="p.id === closestId">
                      {{ spec.read(p) | formatMetric: spec.decimals }}
                    </td>
                  }
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="empty-compare">
          Pin up to 4 exoplanets from the catalog table or Orbital Estimator to compare radius, mass, insolation flux, and host star luminosity side by side with Earth.
        </div>
      }
    </section>
  `,
  styles: `
    .compare-section {
      padding: 16px 18px;
      min-width: 0;
    }
    .compare-head {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }
    .compare-head h2 {
      font-size: 20px;
    }
    .compare-sub {
      margin: 2px 0 0;
      font-size: 12px;
      color: var(--ink-muted);
    }
    .matrix-scroll {
      overflow-x: auto;
      margin-top: 12px;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      min-width: 640px;
      font-size: 13px;
    }
    th,
    td {
      padding: 9px 12px;
      border-bottom: 1px solid var(--rule-subtle);
      text-align: left;
      vertical-align: middle;
    }
    th {
      border-bottom: 1px solid var(--rule-hairline);
    }
    .metric-col {
      font-weight: 600;
      color: var(--ink-secondary);
      min-width: 185px;
    }
    .unit-tag {
      color: var(--ink-muted);
      font-weight: 400;
    }
    .earth-col {
      background: rgba(30, 91, 255, 0.12);
      color: var(--ink-primary);
    }
    .col-head-flex {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 8px;
    }
    .col-title {
      font-family: var(--font-display);
      font-size: 17px;
      color: var(--ink-primary);
    }
    .col-sub {
      font-size: 11px;
      font-weight: 400;
      color: var(--ink-muted);
    }
    .unpin-btn {
      background: none;
      border: 1px solid var(--rule-hairline);
      border-radius: var(--radius-sm);
      color: var(--ink-secondary);
      width: 24px;
      height: 24px;
      line-height: 1;
      cursor: pointer;
    }
    .unpin-btn:hover {
      border-color: var(--route-scarlet);
      color: var(--route-scarlet-text);
    }
    .closest-earth {
      color: var(--route-green-text);
      font-weight: 700;
    }
    .rocky-note {
      margin-left: 6px;
      font-size: 11px;
      color: var(--route-green-text);
      font-weight: 600;
    }
    .empty-compare {
      margin-top: 12px;
      padding: 18px;
      border: 1px dashed var(--rule-hairline);
      border-radius: var(--radius-sm);
      color: var(--ink-secondary);
    }
  `
})
export class ComparePanel {
  readonly pinnedIds = input<number[]>([]);
  readonly comparison = input<ComparisonResponse | null>(null);
  readonly loading = input(false);

  readonly unpinPlanet = output<number>();
  readonly clearPinned = output<void>();
  readonly loadSuggested = output<void>();

  protected readonly metricRows: readonly CompareRowSpec[] = [
    { label: 'Earth Similarity Index', unit: '%', read: (p) => p.earthSimilarityScore, decimals: 1 },
    { label: 'Planet Mass', unit: 'M⊕', read: (p) => p.massEarth, decimals: 2 },
    { label: 'Planet Radius', unit: 'R⊕', read: (p) => p.radiusEarth, decimals: 2 },
    { label: 'Orbital Period', unit: 'days', read: (p) => p.orbitalPeriodDays, decimals: 2 },
    { label: 'Semi-Major Axis', unit: 'AU', read: (p) => p.semiMajorAxisAu, decimals: 4 },
    { label: 'Insolation Flux', unit: 'S⊕', read: (p) => p.insolationEarth, decimals: 3 },
    { label: 'Equilibrium Temp', unit: 'K', read: (p) => p.equilibriumTempK, decimals: 0 },
    { label: 'Stellar Luminosity', unit: 'L☉', read: (p) => p.stellarLuminositySolar, decimals: 4 },
    { label: 'Host Star Temp', unit: 'K', read: (p) => p.stellarTeffK, decimals: 0 },
    { label: 'Distance', unit: 'pc', read: (p) => p.distanceParsecs, decimals: 2 }
  ];

  protected findClosestPlanetId(cmp: ComparisonResponse, spec: CompareRowSpec): number | null {
    const earthVal = spec.read(cmp.earthReference);
    if (earthVal === null || cmp.planets.length < 2) {
      return null;
    }

    let bestId: number | null = null;
    let bestDiff = Number.POSITIVE_INFINITY;

    for (const p of cmp.planets) {
      const val = spec.read(p);
      if (val === null) continue;
      const diff = Math.abs(val - earthVal);
      if (diff < bestDiff) {
        bestDiff = diff;
        bestId = p.id;
      }
    }

    return bestId;
  }
}

