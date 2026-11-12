import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  effect,
  inject,
  input,
  viewChild
} from '@angular/core';
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  Legend,
  LinearScale,
  Tooltip
} from 'chart.js';
import { DiscoveryTrends } from '../../core/models/exoplanet.model';
import { getDiscoveryMethodColor } from '../../shared/pipes/metric-format';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

@Component({
  selector: 'app-discovery-trends-chart',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="enamel-panel chart-card">
      <header class="chart-header">
        <h3>Discovery Year Trends</h3>
        <p class="chart-sub">Confirmed worlds per year stacked by primary detection method</p>
      </header>

      <div class="canvas-stage">
        @if (loading()) {
          <div class="skeleton-block stage-skeleton" aria-label="Loading discovery trends chart"></div>
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
export class DiscoveryTrendsChart {
  private readonly destroyRef = inject(DestroyRef);
  private readonly canvasRef = viewChild<ElementRef<HTMLCanvasElement>>('canvas');

  readonly trends = input<DiscoveryTrends | null>(null);
  readonly loading = input(false);

  private chart: Chart<'bar'> | null = null;

  constructor() {
    effect(() => {
      const canvasEl = this.canvasRef()?.nativeElement;
      const data = this.trends();
      if (!canvasEl || !data) {
        return;
      }

      this.renderChart(canvasEl, data);
    });

    this.destroyRef.onDestroy(() => {
      this.chart?.destroy();
      this.chart = null;
    });
  }

  private renderChart(canvas: HTMLCanvasElement, data: DiscoveryTrends): void {
    const years = Array.from(new Set(data.points.map((p) => p.year))).sort((a, b) => a - b);
    const primaryMethods = data.methods.slice(0, 4);
    const hasOther = data.methods.length > 4;

    const bucketMap = new Map<string, Map<number, number>>();
    for (const m of primaryMethods) {
      bucketMap.set(m, new Map<number, number>());
    }
    if (hasOther) {
      bucketMap.set('Other Methods', new Map<number, number>());
    }

    for (const pt of data.points) {
      const bucketName = primaryMethods.includes(pt.discoveryMethod) ? pt.discoveryMethod : 'Other Methods';
      const map = bucketMap.get(bucketName);
      if (map) {
        map.set(pt.year, (map.get(pt.year) ?? 0) + pt.count);
      }
    }

    const datasets = Array.from(bucketMap.entries()).map(([method, yearCounts]) => ({
      label: method,
      data: years.map((y) => yearCounts.get(y) ?? 0),
      backgroundColor: getDiscoveryMethodColor(method),
      borderWidth: 0
    }));

    this.chart?.destroy();
    this.chart = new Chart<'bar'>(canvas, {
      type: 'bar',
      data: {
        labels: years.map(String),
        datasets
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
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
            bodyColor: '#C8D6EF'
          }
        },
        scales: {
          x: {
            stacked: true,
            ticks: {
              color: '#93A9D2',
              maxRotation: 0,
              autoSkip: true,
              font: { family: 'JetBrains Mono, monospace', size: 10 }
            },
            grid: { display: false }
          },
          y: {
            stacked: true,
            beginAtZero: true,
            title: {
              display: true,
              text: 'Confirmed Discoveries',
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

