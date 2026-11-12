import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, input, output, signal } from '@angular/core';
import {
  createDefaultFilterState,
  ExoplanetFilterState,
  HabitabilityStatus
} from '../../core/models/exoplanet.model';
import { getDiscoveryMethodColor } from '../../shared/pipes/metric-format';

@Component({
  selector: 'app-filter-rail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <aside class="rail" aria-label="Catalog Filters">
      <div class="rail-header">
        <h2>Catalog Filters</h2>
        @if (hasActiveFilters()) {
          <button type="button" class="link-reset" (click)="resetAll()">Reset all</button>
        }
      </div>

      <div class="group">
        <label for="planet-search" class="field-label">Planet or Host Star</label>
        <input
          id="planet-search"
          type="search"
          class="text-input"
          placeholder="e.g. TRAPPIST-1, Kepler-186"
          [value]="filters().search"
          (input)="onSearchInput($event)"
        />
      </div>

      <div class="group">
        <span class="field-label">Quick Presets</span>
        <div class="preset-grid">
          <button type="button" class="chip" (click)="applyPreset('all')">All Worlds</button>
          <button type="button" class="chip" (click)="applyPreset('rocky-hz')">Rocky HZ</button>
          <button type="button" class="chip" (click)="applyPreset('trappist')">TRAPPIST-1</button>
          <button type="button" class="chip" (click)="applyPreset('giants')">Gas Giants</button>
        </div>
      </div>

      <div class="group">
        <span class="field-label">Habitable Zone Status</span>
        <div class="chip-row" role="group" aria-label="Habitable Zone Status">
          @for (opt of hzOptions; track opt.value) {
            <button
              type="button"
              class="chip"
              [class.active]="filters().habitabilityStatus === opt.value"
              [attr.aria-pressed]="filters().habitabilityStatus === opt.value"
              (click)="setHabitability(opt.value)"
            >
              {{ opt.label }}
            </button>
          }
        </div>
        <label class="toggle-row">
          <input
            type="checkbox"
            [checked]="filters().rockyOnly"
            (change)="onRockyToggle($event)"
          />
          <span>Rocky candidates only (&le; 1.8 R&#8853;)</span>
        </label>
      </div>

      <div class="group">
        <label for="method-select" class="field-label">Discovery Method</label>
        <select
          id="method-select"
          class="text-input"
          [value]="filters().discoveryMethod"
          (change)="onMethodChange($event)"
        >
          <option value="">All methods ({{ methods().length }})</option>
          @for (method of methods(); track method) {
            <option [value]="method">{{ method }}</option>
          }
        </select>
        @if (filters().discoveryMethod) {
          <div class="method-indicator">
            <span class="route-dot" [style.background]="methodColor(filters().discoveryMethod)"></span>
            <span>{{ filters().discoveryMethod }}</span>
          </div>
        }
      </div>

      <div class="group">
        <span class="field-label">Planet Mass (M&#8853;)</span>
        <div class="range-pair">
          <input
            type="number"
            min="0"
            step="any"
            aria-label="Minimum mass in Earth masses"
            class="text-input mono-num"
            placeholder="Min"
            [value]="filters().minMass ?? ''"
            (change)="onNumberChange('minMass', $event)"
          />
          <span class="dash" aria-hidden="true">&ndash;</span>
          <input
            type="number"
            min="0"
            step="any"
            aria-label="Maximum mass in Earth masses"
            class="text-input mono-num"
            placeholder="Max"
            [value]="filters().maxMass ?? ''"
            (change)="onNumberChange('maxMass', $event)"
          />
        </div>
      </div>

      <div class="group">
        <span class="field-label">Planet Radius (R&#8853;)</span>
        <div class="range-pair">
          <input
            type="number"
            min="0"
            step="any"
            aria-label="Minimum radius in Earth radii"
            class="text-input mono-num"
            placeholder="Min"
            [value]="filters().minRadius ?? ''"
            (change)="onNumberChange('minRadius', $event)"
          />
          <span class="dash" aria-hidden="true">&ndash;</span>
          <input
            type="number"
            min="0"
            step="any"
            aria-label="Maximum radius in Earth radii"
            class="text-input mono-num"
            placeholder="Max"
            [value]="filters().maxRadius ?? ''"
            (change)="onNumberChange('maxRadius', $event)"
          />
        </div>
      </div>

      <div class="group">
        <span class="field-label">Orbital Period (days)</span>
        <div class="range-pair">
          <input
            type="number"
            min="0"
            step="any"
            aria-label="Minimum orbital period in days"
            class="text-input mono-num"
            placeholder="Min"
            [value]="filters().minPeriod ?? ''"
            (change)="onNumberChange('minPeriod', $event)"
          />
          <span class="dash" aria-hidden="true">&ndash;</span>
          <input
            type="number"
            min="0"
            step="any"
            aria-label="Maximum orbital period in days"
            class="text-input mono-num"
            placeholder="Max"
            [value]="filters().maxPeriod ?? ''"
            (change)="onNumberChange('maxPeriod', $event)"
          />
        </div>
      </div>

      @if (rangeError()) {
        <p class="range-error" role="alert">{{ rangeError() }}</p>
      }
    </aside>
  `,
  styles: `
    .rail {
      background: var(--bg-rail);
      border: 1px solid var(--rule-hairline);
      border-radius: var(--radius-md);
      padding: 18px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    .rail-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-bottom: 10px;
      border-bottom: 1px solid var(--rule-subtle);
    }
    .rail-header h2 {
      font-size: 20px;
    }
    .link-reset {
      background: none;
      border: none;
      color: var(--route-cobalt-text);
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
      padding: 4px 6px;
    }
    .link-reset:hover {
      text-decoration: underline;
    }
    .group {
      display: flex;
      flex-direction: column;
      gap: 7px;
    }
    .field-label {
      font-size: 12px;
      font-weight: 600;
      color: var(--ink-secondary);
    }
    .text-input {
      width: 100%;
      min-height: 36px;
      padding: 6px 10px;
      background: var(--bg-input);
      border: 1px solid var(--rule-hairline);
      border-radius: var(--radius-sm);
      color: var(--ink-primary);
      font-size: 13px;
    }
    .text-input::placeholder {
      color: var(--ink-muted);
    }
    .text-input:focus {
      border-color: var(--route-cobalt-text);
    }
    .preset-grid,
    .chip-row {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }
    .chip {
      min-height: 30px;
      padding: 4px 10px;
      border-radius: var(--radius-sm);
      border: 1px solid var(--rule-hairline);
      background: rgba(255, 255, 255, 0.04);
      color: var(--ink-secondary);
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
    }
    .chip:hover {
      border-color: var(--ink-primary);
      color: var(--ink-primary);
    }
    .chip.active {
      background: var(--route-cobalt);
      border-color: var(--route-cobalt);
      color: #ffffff;
    }
    .toggle-row {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-top: 4px;
      font-size: 12.5px;
      color: var(--ink-secondary);
      cursor: pointer;
    }
    .toggle-row input {
      width: 16px;
      height: 16px;
    }
    .range-pair {
      display: grid;
      grid-template-columns: 1fr auto 1fr;
      align-items: center;
      gap: 6px;
    }
    .dash {
      color: var(--ink-muted);
    }
    .method-indicator {
      display: inline-flex;
      align-items: center;
      gap: 7px;
      font-size: 12px;
      color: var(--ink-secondary);
    }
    .route-dot {
      width: 9px;
      height: 9px;
      border-radius: 50%;
    }
    .range-error {
      margin: 0;
      padding: 8px 10px;
      border-radius: var(--radius-sm);
      background: var(--route-scarlet-bg);
      border: 1px solid var(--route-scarlet);
      color: var(--route-scarlet-text);
      font-size: 12px;
      font-weight: 600;
    }
  `
})
export class FilterRail {
  private readonly destroyRef = inject(DestroyRef);

  readonly filters = input.required<ExoplanetFilterState>();
  readonly methods = input<string[]>([]);
  readonly filtersChanged = output<ExoplanetFilterState>();

  protected readonly rangeError = signal<string | null>(null);
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly hzOptions: ReadonlyArray<{ label: string; value: '' | HabitabilityStatus }> = [
    { label: 'All', value: '' },
    { label: 'Conservative', value: 'Conservative' },
    { label: 'Optimistic', value: 'Optimistic' },
    { label: 'Outside', value: 'Outside' }
  ];

  protected readonly hasActiveFilters = computed(() => {
    const f = this.filters();
    return Boolean(
      f.search.trim() ||
      f.minMass !== null ||
      f.maxMass !== null ||
      f.minRadius !== null ||
      f.maxRadius !== null ||
      f.minPeriod !== null ||
      f.maxPeriod !== null ||
      f.discoveryMethod ||
      f.habitabilityStatus ||
      f.rockyOnly
    );
  });

  constructor() {
    this.destroyRef.onDestroy(() => {
      if (this.searchTimer !== null) {
        clearTimeout(this.searchTimer);
      }
    });
  }

  protected onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    if (this.searchTimer !== null) {
      clearTimeout(this.searchTimer);
    }

    this.searchTimer = setTimeout(() => {
      this.emitNext({ ...this.filters(), search: value, page: 1 });
    }, 280);
  }

  protected setHabitability(status: '' | HabitabilityStatus): void {
    this.emitNext({ ...this.filters(), habitabilityStatus: status, page: 1 });
  }

  protected onRockyToggle(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.emitNext({ ...this.filters(), rockyOnly: checked, page: 1 });
  }

  protected onMethodChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.emitNext({ ...this.filters(), discoveryMethod: value, page: 1 });
  }

  protected onNumberChange(
    key: 'minMass' | 'maxMass' | 'minRadius' | 'maxRadius' | 'minPeriod' | 'maxPeriod',
    event: Event
  ): void {
    const raw = (event.target as HTMLInputElement).value.trim();
    const parsed = raw === '' ? null : Number(raw);
    if (parsed !== null && (!Number.isFinite(parsed) || parsed < 0)) {
      this.rangeError.set('Numeric bounds must be 0 or greater.');
      return;
    }

    const next = { ...this.filters(), [key]: parsed, page: 1 };
    const err = this.validateRanges(next);
    this.rangeError.set(err);
    if (!err) {
      this.filtersChanged.emit(next);
    }
  }

  protected applyPreset(preset: 'all' | 'rocky-hz' | 'trappist' | 'giants'): void {
    const base = createDefaultFilterState();
    switch (preset) {
      case 'rocky-hz':
        this.emitNext({ ...base, habitabilityStatus: 'Conservative', rockyOnly: true, sortBy: 'similarity', sortDir: 'desc' });
        break;
      case 'trappist':
        this.emitNext({ ...base, search: 'TRAPPIST-1', sortBy: 'period', sortDir: 'asc' });
        break;
      case 'giants':
        this.emitNext({ ...base, minMass: 100, sortBy: 'mass', sortDir: 'desc' });
        break;
      default:
        this.emitNext(base);
    }
  }

  protected resetAll(): void {
    this.rangeError.set(null);
    this.filtersChanged.emit(createDefaultFilterState());
  }

  protected methodColor(method: string): string {
    return getDiscoveryMethodColor(method);
  }

  private emitNext(next: ExoplanetFilterState): void {
    const err = this.validateRanges(next);
    this.rangeError.set(err);
    if (!err) {
      this.filtersChanged.emit(next);
    }
  }

  private validateRanges(f: ExoplanetFilterState): string | null {
    if (f.minMass !== null && f.maxMass !== null && f.minMass > f.maxMass) {
      return 'Minimum mass cannot exceed maximum mass.';
    }
    if (f.minRadius !== null && f.maxRadius !== null && f.minRadius > f.maxRadius) {
      return 'Minimum radius cannot exceed maximum radius.';
    }
    if (f.minPeriod !== null && f.maxPeriod !== null && f.minPeriod > f.maxPeriod) {
      return 'Minimum period cannot exceed maximum period.';
    }
    return null;
  }
}

