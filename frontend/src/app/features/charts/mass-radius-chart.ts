import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  effect,
  inject,
  input,
  output,
  viewChild
} from '@angular/core';
import {
  Chart,
  Legend,
  LogarithmicScale,
  PointElement,
  ScatterController,
  Tooltip
} from 'chart.js';
import { MassRadiusChart as MassRadiusChartPayload, MassRadiusPoint } from '../../core/models/exoplanet.model';

Chart.register(ScatterController, LogarithmicScale, PointElement, Tooltip, Legend);

interface ScatterDatum {
  x: number;
  y: number;
  meta?: MassRadiusPoint;
  label?: string;
}

@Component({
  selector: 'app-mass-radius-chart',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="enamel-panel chart-card">
      <header class="chart-header">
        <div>
          <h3>Mass vs. Radius (Log–Log)</h3>
          <p class="chart-sub">
            @if (chartData(); as data) {
              Showing <span class="mono-num">{{ data.returnedCount.toLocaleString() }}</span>
              @if (data.returnedCount < data.totalMatching) {
                of <span class="mono-num">{{ data.totalMatching.toLocaleString() }}</span>
              }
              confirmed worlds &middot; click any point to inspect orbit
            } @else {
              Earth masses (M&#8853;) vs. Earth radii (R&#8853;)
            }
          </p>
        </div>
      </header>

      <div class="canvas-stage">
        @if (loading()) {
          <div class="skeleton-block stage-skeleton" aria-label="Loading scatter chart"></div>
        }
        <canvas #canvas [class.hidden]="loading()"></canvas>
      </div>
    </article>
  `,
  styles: `
    .chart-card {
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 10px;
      min-width: 0;
    }
    .chart-header h3 {
      font-size: 19px;
      line-height: 1.15;
    }
    .chart-sub {
      margin: 2px 0 0;
      font-size: 12px;
      color: var(--ink-muted);
    }
    .canvas-stage {
      position: relative;
      height: 290px;
      width: 100%;
    }
    .stage-skeleton {
      width: 100%;
      height: 100%;
    }
    canvas.hidden {
      display: none;
    }
  `
})
export class MassRadiusChart {
  private readonly destroyRef = inject(DestroyRef);
  private readonly canvasRef = viewChild<ElementRef<HTMLCanvasElement>>('canvas');

  readonly chartData = input<MassRadiusChartPayload | null>(null);
  readonly selectedId = input<number | null>(null);
  readonly loading = input(false);
  readonly planetSelected = output<number>();

  private chart: Chart<'scatter', ScatterDatum[]> | null = null;

  constructor() {
    effect(() => {
      const canvasEl = this.canvasRef()?.nativeElement;
      const data = this.chartData();
      const selected = this.selectedId();
      if (!canvasEl || !data) {
        return;
      }

      this.renderChart(canvasEl, data.points, selected);
    });

    this.destroyRef.onDestroy(() => {
      this.chart?.destroy();
      this.chart = null;
    });
  }

  private renderChart(canvas: HTMLCanvasElement, points: MassRadiusPoint[], selectedId: number | null): void {
    const conservative: ScatterDatum[] = [];
    const optimistic: ScatterDatum[] = [];
    const catalog: ScatterDatum[] = [];
    const selectedPts: ScatterDatum[] = [];

    for (const pt of points) {
      const item: ScatterDatum = { x: pt.massEarth, y: pt.radiusEarth, meta: pt };
      if (pt.id === selectedId) {
        selectedPts.push(item);
      } else if (pt.habitabilityStatus === 'Conservative') {
        conservative.push(item);
      } else if (pt.habitabilityStatus === 'Optimistic') {
        optimistic.push(item);
      } else {
        catalog.push(item);
      }
    }

    const solarRefs: ScatterDatum[] = [
      { x: 1, y: 1, label: 'Earth (1.0 M⊕, 1.0 R⊕)' },
      { x: 17.15, y: 3.88, label: 'Neptune (17.15 M⊕, 3.88 R⊕)' },
      { x: 317.8, y: 11.21, label: 'Jupiter (317.8 M⊕, 11.21 R⊕)' }
    ];

    this.chart?.destroy();
    this.chart = new Chart<'scatter', ScatterDatum[]>(canvas, {
      type: 'scatter',
      data: {
        datasets: [
          {
            label: 'Catalog Worlds',
            data: catalog,
            backgroundColor: 'rgba(78, 142, 255, 0.48)',
            borderColor: 'rgba(122, 168, 255, 0.72)',
            borderWidth: 1,
            pointRadius: 3.2,
            pointHoverRadius: 6
          },
          {
            label: 'Optimistic HZ',
            data: optimistic,
            backgroundColor: '#FFC20E',
            borderColor: '#FFFFFF',
            borderWidth: 1,
            pointRadius: 5,
            pointHoverRadius: 7.5
          },
          {
            label: 'Conservative HZ',
            data: conservative,
            backgroundColor: '#22B14C',
            borderColor: '#FFFFFF',
            borderWidth: 1.2,
            pointRadius: 5.5,
            pointHoverRadius: 8
          },
          {
            label: 'Selected World',
            data: selectedPts,
            backgroundColor: '#E21D2D',
            borderColor: '#FFFFFF',
            borderWidth: 2,
            pointRadius: 8,
            pointHoverRadius: 9.5
          },
          {
            label: 'Solar Baseline',
            data: solarRefs,
            backgroundColor: '#FFFFFF',
            borderColor: '#0B1230',
            borderWidth: 1.5,
            pointStyle: 'rectRot',
            pointRadius: 6.5,
            pointHoverRadius: 8
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        onClick: (_, elements) => {
          if (elements.length === 0 || !this.chart) return;
          const first = elements[0];
          const ds = this.chart.data.datasets[first.datasetIndex];
          const datum = ds.data[first.index];
          if (datum?.meta?.id) {
            this.planetSelected.emit(datum.meta.id);
          }
        },
        plugins: {
          legend: {
            labels: {
              color: '#C8D6EF',
              boxWidth: 10,
              boxHeight: 10,
              font: { family: 'Public Sans, sans-serif', size: 11 }
            }
          },
          tooltip: {
            backgroundColor: '#080F28',
            borderColor: 'rgba(255,255,255,0.28)',
            borderWidth: 1,
            titleColor: '#FFFFFF',
            bodyColor: '#C8D6EF',
            callbacks: {
              label: (ctx) => {
                const raw = ctx.raw as ScatterDatum;
                if (raw.label) return raw.label;
                if (!raw.meta) return '';
                const m = raw.meta;
                return `${m.planetName} (${m.habitabilityStatus}) · ${m.massEarth.toFixed(2)} M⊕ · ${m.radiusEarth.toFixed(2)} R⊕`;
              }
            }
          }
        },
        scales: {
          x: {
            type: 'logarithmic',
            title: {
              display: true,
              text: 'Planet Mass (M⊕)',
              color: '#93A9D2',
              font: { size: 11 }
            },
            ticks: {
              color: '#93A9D2',
              font: { family: 'JetBrains Mono, monospace', size: 10 }
            },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          },
          y: {
            type: 'logarithmic',
            title: {
              display: true,
              text: 'Planet Radius (R⊕)',
              color: '#93A9D2',
              font: { size: 11 }
            },
            ticks: {
              color: '#93A9D2',
              font: { family: 'JetBrains Mono, monospace', size: 10 }
            },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          }
        }
      }
    });
  }
}

