import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, forkJoin, of, switchMap } from 'rxjs';
import {
  CatalogSummary,
  ComparisonResponse,
  DiscoveryTrends,
  ExoplanetDetail,
  ExoplanetFilterState,
  ExoplanetListItem,
  MassRadiusChart as MassRadiusChartData,
  PagedResult,
  SyncStatusResponse,
  createDefaultFilterState
} from '../../core/models/exoplanet.model';
import { ExoplanetApi } from '../../core/services/exoplanet-api';
import { CatalogTable } from '../catalog-table/catalog-table';
import { DiscoveryTrendsChart } from '../charts/discovery-trends-chart';
import { MassRadiusChart } from '../charts/mass-radius-chart';
import { ComparePanel } from '../compare-panel/compare-panel';
import { FilterRail } from '../filters/filter-rail';
import { HabitableCorridor } from '../habitable-corridor/habitable-corridor';
import { SyncPanel } from '../sync-drawer/sync-panel';

@Component({
  selector: 'app-observatory-dashboard',
  imports: [
    DatePipe,
    FilterRail,
    HabitableCorridor,
    MassRadiusChart,
    DiscoveryTrendsChart,
    CatalogTable,
    ComparePanel,
    SyncPanel
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="shell">
      <header class="topbar">
        <div class="brand-group">
          <svg class="brand-mark" viewBox="0 0 32 32" width="28" height="28" aria-hidden="true">
            <line x1="3" y1="26" x2="14" y2="15" stroke="#22B14C" stroke-width="2.6" stroke-linecap="round" />
            <line x1="18" y1="13" x2="29" y2="6" stroke="#FFC20E" stroke-width="2.6" stroke-linecap="round" />
            <circle cx="16" cy="14" r="5" fill="#0B1230" stroke="#FFFFFF" stroke-width="2.6" />
          </svg>
          <h1>Exoplanet Explorer</h1>
        </div>

        <div class="telemetry-strip" aria-label="Catalog Telemetry">
          @if (summary(); as s) {
            <div class="stat-pill">
              <span class="stat-label">Confirmed</span>
              <span class="mono-num stat-val">{{ s.totalExoplanets.toLocaleString() }}</span>
            </div>
            <div class="stat-pill">
              <span class="stat-label">Conservative HZ</span>
              <span class="mono-num stat-val green">{{ s.conservativeHzCount.toLocaleString() }}</span>
            </div>
            <div class="stat-pill">
              <span class="stat-label">Rocky HZ</span>
              <span class="mono-num stat-val green">{{ s.rockyHzCount.toLocaleString() }}</span>
            </div>
            <div class="stat-pill hide-compact">
              <span class="stat-label">Host Stars</span>
              <span class="mono-num stat-val">{{ s.distinctHostStars.toLocaleString() }}</span>
            </div>
            <div class="stat-pill hide-compact">
              <span class="stat-label">Last NASA Sync</span>
              <span class="mono-num stat-val">
                {{ s.lastSyncedUtc ? (s.lastSyncedUtc | date: 'MMM d, HH:mm':'UTC') + ' UTC' : 'Pending' }}
              </span>
            </div>
          } @else {
            <div class="skeleton-block" style="width: 340px; height: 26px;"></div>
          }
        </div>

        <div class="top-actions">
          <button
            type="button"
            class="btn-transit mobile-filter-btn"
            (click)="mobileFiltersOpen.set(!mobileFiltersOpen())"
            [attr.aria-expanded]="mobileFiltersOpen()"
          >
            {{ mobileFiltersOpen() ? 'Hide Filters' : 'Filters' }}
          </button>
          <button
            type="button"
            class="btn-transit"
            (click)="showSyncLog.set(!showSyncLog())"
            [attr.aria-expanded]="showSyncLog()"
          >
            Sync Log
          </button>
          <button
            type="button"
            class="btn-transit btn-scarlet"
            [disabled]="isSyncing()"
            (click)="triggerManualSync()"
          >
            {{ isSyncing() ? 'Syncing NASA...' : 'Sync NASA Archive' }}
          </button>
        </div>
      </header>

      @if (showSyncLog()) {
        <div class="drawer-slot">
          <app-sync-panel
            [status]="syncStatus()"
            [syncError]="syncError()"
            (closeRequested)="showSyncLog.set(false)"
          />
        </div>
      }

      @if (bannerNotice(); as notice) {
        <div class="notice-bar" [class.notice-error]="notice.isError" role="status">
          <span>{{ notice.message }}</span>
          <button type="button" class="notice-close" aria-label="Dismiss notice" (click)="bannerNotice.set(null)">
            &times;
          </button>
        </div>
      }

      <div class="workspace">
        <div class="rail-slot" [class.mobile-open]="mobileFiltersOpen()">
          <app-filter-rail
            [filters]="filters()"
            [methods]="summary()?.discoveryMethods ?? []"
            (filtersChanged)="onFiltersChanged($event)"
          />
        </div>

        <main class="main-column">
          <app-habitable-corridor
            [planet]="selectedPlanet()"
            [loading]="detailLoading()"
            [isPinned]="selectedPlanet() ? pinnedIds().includes(selectedPlanet()!.id) : false"
            (pinCompare)="togglePin($event)"
          />

          <div class="charts-grid">
            <app-mass-radius-chart
              [chartData]="massRadiusChart()"
              [selectedId]="selectedPlanetId()"
              [loading]="queryLoading()"
              (planetSelected)="selectPlanet($event)"
            />
            <app-discovery-trends-chart
              [trends]="discoveryTrends()"
              [loading]="queryLoading()"
            />
          </div>

          <app-catalog-table
            [data]="catalog()"
            [loading]="queryLoading()"
            [error]="queryError()"
            [selectedId]="selectedPlanetId()"
            [pinnedIds]="pinnedIds()"
            [sortBy]="filters().sortBy"
            [sortDir]="filters().sortDir"
            (sortChanged)="onSortChanged($event)"
            (pageChanged)="onPageChanged($event)"
            (planetSelected)="selectPlanet($event)"
            (pinToggled)="togglePin($event)"
            (retryRequested)="reloadWorkspaceData()"
            (resetFiltersRequested)="resetFilters()"
          />

          <app-compare-panel
            [pinnedIds]="pinnedIds()"
            [comparison]="comparison()"
            [loading]="compareLoading()"
            (unpinPlanet)="unpin($event)"
            (clearPinned)="clearComparison()"
            (loadSuggested)="loadSuggestedComparison()"
          />
        </main>
      </div>
    </div>
  `,
  styles: `
    .shell {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }
    .topbar {
      position: sticky;
      top: 0;
      z-index: 20;
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 12px 20px;
      padding: 10px 20px;
      background: rgba(11, 18, 48, 0.94);
      backdrop-filter: blur(10px);
      border-bottom: 1px solid var(--rule-hairline);
    }
    .brand-group {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .brand-group h1 {
      font-size: 24px;
      line-height: 1;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .telemetry-strip {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 18px;
    }
    .stat-pill {
      display: flex;
      align-items: baseline;
      gap: 6px;
    }
    .stat-label {
      font-size: 12px;
      color: var(--ink-muted);
    }
    .stat-val {
      font-size: 14px;
      font-weight: 700;
      color: var(--ink-primary);
    }
    .stat-val.green {
      color: var(--route-green-text);
    }
    .top-actions {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .mobile-filter-btn {
      display: none;
    }
    .drawer-slot {
      padding: 12px 20px 0;
    }
    .notice-bar {
      margin: 10px 20px 0;
      padding: 10px 14px;
      border-radius: var(--radius-sm);
      border: 1px solid rgba(34, 177, 76, 0.5);
      background: var(--route-green-bg);
      color: var(--route-green-text);
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      font-size: 13px;
      font-weight: 600;
    }
    .notice-bar.notice-error {
      border-color: var(--route-scarlet);
      background: var(--route-scarlet-bg);
      color: var(--route-scarlet-text);
    }
    .notice-close {
      background: none;
      border: none;
      color: inherit;
      font-size: 18px;
      line-height: 1;
      cursor: pointer;
    }
    .workspace {
      display: grid;
      grid-template-columns: 280px minmax(0, 1fr);
      gap: 18px;
      padding: 18px 20px 28px;
      align-items: start;
    }
    .rail-slot {
      position: sticky;
      top: 72px;
    }
    .main-column {
      display: flex;
      flex-direction: column;
      gap: 18px;
      min-width: 0;
    }
    .charts-grid {
      display: grid;
      grid-template-columns: 1.2fr 1fr;
      gap: 18px;
      min-width: 0;
    }
    @media (max-width: 1180px) {
      .charts-grid {
        grid-template-columns: 1fr;
      }
      .hide-compact {
        display: none;
      }
    }
    @media (max-width: 960px) {
      .mobile-filter-btn {
        display: inline-flex;
      }
      .workspace {
        grid-template-columns: 1fr;
      }
      .rail-slot {
        position: static;
        display: none;
      }
      .rail-slot.mobile-open {
        display: block;
      }
    }
  `
})
export class ObservatoryDashboard implements OnInit {
  private readonly api = inject(ExoplanetApi);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly filters = signal<ExoplanetFilterState>(createDefaultFilterState());
  protected readonly summary = signal<CatalogSummary | null>(null);
  protected readonly catalog = signal<PagedResult<ExoplanetListItem> | null>(null);
  protected readonly massRadiusChart = signal<MassRadiusChartData | null>(null);
  protected readonly discoveryTrends = signal<DiscoveryTrends | null>(null);
  protected readonly selectedPlanetId = signal<number | null>(null);
  protected readonly selectedPlanet = signal<ExoplanetDetail | null>(null);
  protected readonly pinnedIds = signal<number[]>([]);
  protected readonly comparison = signal<ComparisonResponse | null>(null);
  protected readonly syncStatus = signal<SyncStatusResponse | null>(null);

  protected readonly queryLoading = signal(true);
  protected readonly detailLoading = signal(false);
  protected readonly compareLoading = signal(false);
  protected readonly isSyncing = signal(false);
  protected readonly showSyncLog = signal(false);
  protected readonly mobileFiltersOpen = signal(false);
  protected readonly queryError = signal<string | null>(null);
  protected readonly syncError = signal<string | null>(null);
  protected readonly bannerNotice = signal<{ message: string; isError: boolean } | null>(null);

  private readonly filterTrigger$ = new Subject<ExoplanetFilterState>();
  private readonly detailTrigger$ = new Subject<number>();
  private readonly compareTrigger$ = new Subject<number[]>();

  constructor() {
    this.filterTrigger$
      .pipe(
        switchMap((state) => {
          this.queryLoading.set(true);
          this.queryError.set(null);
          return forkJoin({
            catalog: this.api.getExoplanets(state),
            scatter: this.api.getMassRadiusChart(state),
            trends: this.api.getDiscoveryTrends(state)
          }).pipe(
            catchError(() => {
              this.queryError.set(
                'Could not load exoplanet catalog from the ASP.NET Core API. Verify the backend is running.'
              );
              return of(null);
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res) => {
        this.queryLoading.set(false);
        if (!res) return;

        this.catalog.set(res.catalog);
        this.massRadiusChart.set(res.scatter);
        this.discoveryTrends.set(res.trends);

        if (this.selectedPlanetId() === null && res.catalog.items.length > 0) {
          this.selectPlanet(res.catalog.items[0].id);
        }
      });

    this.detailTrigger$
      .pipe(
        switchMap((id) => {
          this.detailLoading.set(true);
          return this.api.getExoplanetById(id).pipe(catchError(() => of(null)));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((detail) => {
        this.detailLoading.set(false);
        if (detail) {
          this.selectedPlanet.set(detail);
        }
      });

    this.compareTrigger$
      .pipe(
        switchMap((ids) => {
          if (ids.length === 0) {
            this.compareLoading.set(false);
            return of(null);
          }
          this.compareLoading.set(true);
          return this.api.compareExoplanets(ids).pipe(
            catchError(() => {
              this.bannerNotice.set({
                message: 'Unable to load planet comparison matrix.',
                isError: true
              });
              return of(null);
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res) => {
        this.compareLoading.set(false);
        this.comparison.set(res);
      });
  }

  ngOnInit(): void {
    this.loadMetadata();
    this.filterTrigger$.next(this.filters());
  }

  protected onFiltersChanged(next: ExoplanetFilterState): void {
    this.filters.set(next);
    this.filterTrigger$.next(next);
  }

  protected onSortChanged(sort: { sortBy: string; sortDir: 'asc' | 'desc' }): void {
    const next = { ...this.filters(), sortBy: sort.sortBy, sortDir: sort.sortDir, page: 1 };
    this.onFiltersChanged(next);
  }

  protected onPageChanged(page: number): void {
    const next = { ...this.filters(), page };
    this.onFiltersChanged(next);
  }

  protected resetFilters(): void {
    this.onFiltersChanged(createDefaultFilterState());
  }

  protected reloadWorkspaceData(): void {
    this.loadMetadata();
    this.filterTrigger$.next(this.filters());
  }

  protected selectPlanet(id: number): void {
    this.selectedPlanetId.set(id);
    this.detailTrigger$.next(id);
  }

  protected togglePin(id: number): void {
    const current = this.pinnedIds();
    if (current.includes(id)) {
      this.unpin(id);
      return;
    }

    if (current.length >= 4) {
      this.bannerNotice.set({
        message: 'Comparison dock holds up to 4 planets alongside Earth. Remove one to pin another.',
        isError: true
      });
      return;
    }

    const updated = [...current, id];
    this.pinnedIds.set(updated);
    this.refreshComparison(updated);
  }

  protected unpin(id: number): void {
    const updated = this.pinnedIds().filter((x) => x !== id);
    this.pinnedIds.set(updated);
    this.refreshComparison(updated);
  }

  protected clearComparison(): void {
    this.pinnedIds.set([]);
    this.comparison.set(null);
  }

  protected loadSuggestedComparison(): void {
    const items = this.catalog()?.items ?? [];
    if (items.length === 0) return;

    const topIds = items.slice(0, 2).map((p) => p.id);
    this.pinnedIds.set(topIds);
    this.refreshComparison(topIds);
  }

  protected triggerManualSync(): void {
    if (this.isSyncing()) return;

    this.isSyncing.set(true);
    this.syncError.set(null);

    this.api
      .triggerSync()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (run) => {
          this.isSyncing.set(false);
          this.bannerNotice.set({
            message: `NASA sync completed: ${run.fetchedCount.toLocaleString()} fetched, ${run.insertedCount.toLocaleString()} added, ${run.updatedCount.toLocaleString()} updated.`,
            isError: false
          });
          this.reloadWorkspaceData();
        },
        error: (err) => {
          this.isSyncing.set(false);
          const msg = err?.error?.detail || 'NASA Archive synchronization failed.';
          this.syncError.set(msg);
          this.showSyncLog.set(true);
          this.loadSyncStatus();
        }
      });
  }

  private refreshComparison(ids: number[]): void {
    this.compareTrigger$.next(ids);
  }

  private loadMetadata(): void {
    this.api
      .getSummary()
      .pipe(catchError(() => of(null)), takeUntilDestroyed(this.destroyRef))
      .subscribe((s) => {
        if (s) this.summary.set(s);
      });

    this.loadSyncStatus();
  }

  private loadSyncStatus(): void {
    this.api
      .getSyncStatus()
      .pipe(catchError(() => of(null)), takeUntilDestroyed(this.destroyRef))
      .subscribe((status) => {
        if (status) {
          this.syncStatus.set(status);
          this.isSyncing.set(status.isSyncRunning);
        }
      });
  }
}
