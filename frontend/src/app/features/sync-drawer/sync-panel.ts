import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { SyncStatusResponse } from '../../core/models/exoplanet.model';

@Component({
  selector: 'app-sync-panel',
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="enamel-panel sync-drawer" aria-label="NASA Archive Synchronization Log">
      <header class="sync-head">
        <div>
          <h3>NASA Exoplanet Archive Sync Schedule &amp; History</h3>
          <p class="sync-sub">
            Nightly background sync runs automatically at 02:00 UTC via ASP.NET Core Hosted Service.
          </p>
        </div>
        <button type="button" class="btn-transit" (click)="closeRequested.emit()">Close</button>
      </header>

      @if (syncError()) {
        <p class="sync-error" role="alert">{{ syncError() }}</p>
      }

      @if (status(); as s) {
        @if (s.recentRuns.length === 0) {
          <p class="empty-note">No synchronization runs recorded yet. Trigger a sync to fetch NASA's pscomppars catalog.</p>
        } @else {
          <div class="sync-scroll">
            <table>
              <thead>
                <tr>
                  <th>Started (UTC)</th>
                  <th>Trigger</th>
                  <th>Status</th>
                  <th class="num-col">Fetched</th>
                  <th class="num-col">Inserted</th>
                  <th class="num-col">Updated</th>
                  <th class="num-col">Unchanged</th>
                </tr>
              </thead>
              <tbody>
                @for (run of s.recentRuns; track run.id) {
                  <tr>
                    <td class="mono-num">{{ run.startedUtc | date: 'yyyy-MM-dd HH:mm:ss':'UTC' }}</td>
                    <td>{{ run.triggerType }}</td>
                    <td>
                      <span
                        class="status-pill"
                        [class.conservative]="run.status === 'Succeeded'"
                        [class.optimistic]="run.status === 'Running'"
                        [class.unknown]="run.status === 'Failed'"
                      >
                        {{ run.status }}
                      </span>
                    </td>
                    <td class="mono-num num-col">{{ run.fetchedCount.toLocaleString() }}</td>
                    <td class="mono-num num-col">{{ run.insertedCount.toLocaleString() }}</td>
                    <td class="mono-num num-col">{{ run.updatedCount.toLocaleString() }}</td>
                    <td class="mono-num num-col">{{ run.unchangedCount.toLocaleString() }}</td>
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
    .sync-drawer {
      padding: 16px 20px;
      background: var(--bg-panel-elevated);
    }
    .sync-head {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 12px;
    }
    .sync-head h3 {
      font-size: 19px;
    }
    .sync-sub {
      margin: 2px 0 0;
      font-size: 12px;
      color: var(--ink-muted);
    }
    .sync-error {
      margin: 10px 0 0;
      padding: 10px 12px;
      background: var(--route-scarlet-bg);
      border: 1px solid var(--route-scarlet);
      border-radius: var(--radius-sm);
      color: var(--route-scarlet-text);
      font-size: 13px;
    }
    .empty-note {
      margin: 12px 0 0;
      color: var(--ink-secondary);
    }
    .sync-scroll {
      overflow-x: auto;
      margin-top: 12px;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 12.5px;
      min-width: 580px;
    }
    th {
      text-align: left;
      padding: 7px 10px;
      border-bottom: 1px solid var(--rule-hairline);
      color: var(--ink-secondary);
    }
    td {
      padding: 8px 10px;
      border-bottom: 1px solid var(--rule-subtle);
      color: var(--ink-secondary);
    }
    .num-col {
      text-align: right;
    }
  `
})
export class SyncPanel {
  readonly status = input<SyncStatusResponse | null>(null);
  readonly syncError = input<string | null>(null);
  readonly closeRequested = output<void>();
}

