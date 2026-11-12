import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ExoplanetListItem, PagedResult } from '../../core/models/exoplanet.model';
import { FormatMetricPipe } from '../../shared/pipes/format-metric-pipe';
import { getDiscoveryMethodColor } from '../../shared/pipes/metric-format';

@Component({
  selector: 'app-catalog-table',
  imports: [FormatMetricPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="enamel-panel table-section" aria-label="Confirmed Exoplanets Catalog">
      <header class="table-header">
        <div>
          <h2>Confirmed Exoplanets Catalog</h2>
          <p class="table-sub">
            @if (data(); as d) {
              <span class="mono-num">{{ d.totalCount.toLocaleString() }}</span> worlds matching current filters
            } @else {
              Browse, sort, and pin worlds for side-by-side comparison
            }
          </p>
        </div>

        @if (data(); as d) {
          <div class="pager">
            <button
              type="button"
              class="btn-transit"
              [disabled]="loading() || d.page <= 1"
              (click)="pageChanged.emit(d.page - 1)"
            >
              Previous
            </button>
            <span class="mono-num page-ind">Page {{ d.page }} / {{ d.totalPages }}</span>
            <button
              type="button"
              class="btn-transit"
              [disabled]="loading() || d.page >= d.totalPages"
              (click)="pageChanged.emit(d.page + 1)"
            >
              Next
            </button>
          </div>
        }
      </header>

      @if (error()) {
        <div class="state-box error-box" role="alert">
          <p>{{ error() }}</p>
          <button type="button" class="btn-transit" (click)="retryRequested.emit()">Retry Query</button>
        </div>
      } @else if (loading() && !data()) {
        <div class="skeleton-rows">
          @for (i of [1, 2, 3, 4, 5, 6]; track i) {
            <div class="skeleton-block" style="height: 36px; width: 100%;"></div>
          }
        </div>
      } @else if (data(); as d) {
        @if (d.items.length === 0) {
          <div class="state-box">
            <p>No confirmed exoplanets match those filter bounds.</p>
            <button type="button" class="btn-transit btn-cobalt" (click)="resetFiltersRequested.emit()">
              Clear All Filters
            </button>
          </div>
        } @else {
          <div class="table-scroll">
            <table>
              <thead>
                <tr>
                  <th>
                    <button type="button" class="sort-btn" (click)="toggleSort('name')" [attr.aria-sort]="ariaSort('name')">
                      Planet {{ sortArrow('name') }}
                    </button>
                  </th>
                  <th>
                    <button type="button" class="sort-btn" (click)="toggleSort('host')" [attr.aria-sort]="ariaSort('host')">
                      Host {{ sortArrow('host') }}
                    </button>
                  </th>
                  <th>
                    <button type="button" class="sort-btn" (click)="toggleSort('method')" [attr.aria-sort]="ariaSort('method')">
                      Method {{ sortArrow('method') }}
                    </button>
                  </th>
                  <th class="num-col">
                    <button type="button" class="sort-btn" (click)="toggleSort('year')" [attr.aria-sort]="ariaSort('year')">
                      Year {{ sortArrow('year') }}
                    </button>
                  </th>
                  <th class="num-col">
                    <button type="button" class="sort-btn" (click)="toggleSort('mass')" [attr.aria-sort]="ariaSort('mass')">
                      Mass (M&#8853;) {{ sortArrow('mass') }}
                    </button>
                  </th>
                  <th class="num-col">
                    <button type="button" class="sort-btn" (click)="toggleSort('radius')" [attr.aria-sort]="ariaSort('radius')">
                      Radius (R&#8853;) {{ sortArrow('radius') }}
                    </button>
                  </th>
                  <th class="num-col">
                    <button type="button" class="sort-btn" (click)="toggleSort('period')" [attr.aria-sort]="ariaSort('period')">
                      Period (d) {{ sortArrow('period') }}
                    </button>
                  </th>
                  <th class="num-col">
                    <button type="button" class="sort-btn" (click)="toggleSort('insolation')" [attr.aria-sort]="ariaSort('insolation')">
                      Flux (S&#8853;) {{ sortArrow('insolation') }}
                    </button>
                  </th>
                  <th>Habitable Zone</th>
                  <th class="num-col">
                    <button type="button" class="sort-btn" (click)="toggleSort('similarity')" [attr.aria-sort]="ariaSort('similarity')">
                      ESI {{ sortArrow('similarity') }}
                    </button>
                  </th>
                  <th>Compare</th>
                </tr>
              </thead>
              <tbody>
                @for (row of d.items; track row.id) {
                  <tr
                    [class.selected]="row.id === selectedId()"
                    (click)="planetSelected.emit(row.id)"
                  >
                    <td class="planet-name">
                      <button
                        type="button"
                        class="row-select-btn"
                        [attr.aria-label]="'Inspect habitable zone for ' + row.planetName"
                        (click)="planetSelected.emit(row.id); $event.stopPropagation()"
                      >
                        {{ row.planetName }}
                      </button>
                    </td>
                    <td>{{ row.hostName }}</td>
                    <td>
                      <span class="method-cell">
                        <span class="route-dot" [style.background]="methodColor(row.discoveryMethod)" aria-hidden="true"></span>
                        {{ row.discoveryMethod }}
                      </span>
                    </td>
                    <td class="mono-num num-col">{{ row.discoveryYear ?? '—' }}</td>
                    <td class="mono-num num-col">{{ row.massEarth | formatMetric: 2 }}</td>
                    <td class="mono-num num-col">{{ row.radiusEarth | formatMetric: 2 }}</td>
                    <td class="mono-num num-col">{{ row.orbitalPeriodDays | formatMetric: 2 }}</td>
                    <td class="mono-num num-col">{{ row.insolationEarth | formatMetric: 2 }}</td>
                    <td>
                      <span class="status-pill" [class]="row.habitabilityStatus.toLowerCase()">
                        {{ row.habitabilityStatus }}
                      </span>
                    </td>
                    <td class="mono-num num-col">
                      {{ row.earthSimilarityScore !== null ? (row.earthSimilarityScore | formatMetric: 1) + '%' : '—' }}
                    </td>
                    <td>
                      <button
                        type="button"
                        class="pin-btn"
                        [class.pinned]="isPinned(row.id)"
                        [disabled]="!isPinned(row.id) && pinnedIds().length >= 4"
                        (click)="pinToggled.emit(row.id); $event.stopPropagation()"
                        [attr.aria-pressed]="isPinned(row.id)"
                      >
                        {{ isPinned(row.id) ? 'Pinned' : 'Pin' }}
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }
    </section>
  `,
  styles: `
    .table-section {
      padding: 16px 18px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      min-width: 0;
    }
    .table-header {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }
    .table-header h2 {
      font-size: 20px;
    }
    .table-sub {
      margin: 2px 0 0;
      font-size: 12px;
      color: var(--ink-muted);
    }
    .pager {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .page-ind {
      font-size: 12px;
      color: var(--ink-secondary);
    }
    .table-scroll {
      overflow-x: auto;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
      min-width: 860px;
    }
    th {
      text-align: left;
      border-bottom: 1px solid var(--rule-hairline);
      padding: 8px 8px;
      color: var(--ink-secondary);
      font-weight: 600;
      white-space: nowrap;
    }
    td {
      padding: 9px 8px;
      border-bottom: 1px solid var(--rule-subtle);
      color: var(--ink-secondary);
      white-space: nowrap;
    }
    tbody tr {
      cursor: pointer;
      transition: background-color 140ms ease-out;
    }
    tbody tr:hover {
      background: rgba(255, 255, 255, 0.05);
    }
    tbody tr.selected {
      background: rgba(30, 91, 255, 0.22);
    }
    tbody tr.selected td {
      color: var(--ink-primary);
    }
    .num-col {
      text-align: right;
    }
    .sort-btn {
      background: none;
      border: none;
      color: inherit;
      font: inherit;
      cursor: pointer;
      padding: 2px 0;
    }
    .sort-btn:hover {
      color: var(--ink-primary);
    }
    .row-select-btn {
      background: none;
      border: none;
      color: var(--ink-primary);
      font-weight: 600;
      font-size: 13px;
      cursor: pointer;
      padding: 0;
      text-align: left;
    }
    .row-select-btn:hover {
      text-decoration: underline;
    }
    .method-cell {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }
    .route-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      flex-shrink: 0;
    }
    .pin-btn {
      min-height: 28px;
      padding: 2px 10px;
      border-radius: var(--radius-sm);
      border: 1px solid var(--rule-hairline);
      background: transparent;
      color: var(--ink-primary);
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
    }
    .pin-btn.pinned {
      background: var(--route-green);
      border-color: var(--route-green);
      color: #041d0c;
    }
    .pin-btn:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }
    .skeleton-rows {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .state-box {
      padding: 24px;
      text-align: center;
      border: 1px dashed var(--rule-hairline);
      border-radius: var(--radius-sm);
      color: var(--ink-secondary);
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
    }
    .error-box {
      border-color: var(--route-scarlet);
      background: var(--route-scarlet-bg);
      color: var(--route-scarlet-text);
    }
  `
})
export class CatalogTable {
  readonly data = input<PagedResult<ExoplanetListItem> | null>(null);
  readonly loading = input(false);
  readonly error = input<string | null>(null);
  readonly selectedId = input<number | null>(null);
  readonly pinnedIds = input<number[]>([]);
  readonly sortBy = input('similarity');
  readonly sortDir = input<'asc' | 'desc'>('desc');

  readonly sortChanged = output<{ sortBy: string; sortDir: 'asc' | 'desc' }>();
  readonly pageChanged = output<number>();
  readonly planetSelected = output<number>();
  readonly pinToggled = output<number>();
  readonly retryRequested = output<void>();
  readonly resetFiltersRequested = output<void>();

  protected isPinned(id: number): boolean {
    return this.pinnedIds().includes(id);
  }

  protected toggleSort(column: string): void {
    const nextDir: 'asc' | 'desc' =
      this.sortBy() === column && this.sortDir() === 'desc' ? 'asc' : 'desc';
    this.sortChanged.emit({ sortBy: column, sortDir: nextDir });
  }

  protected ariaSort(column: string): 'ascending' | 'descending' | 'none' {
    if (this.sortBy() !== column) return 'none';
    return this.sortDir() === 'asc' ? 'ascending' : 'descending';
  }

  protected sortArrow(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDir() === 'asc' ? '↑' : '↓';
  }

  protected methodColor(method: string): string {
    return getDiscoveryMethodColor(method);
  }
}
