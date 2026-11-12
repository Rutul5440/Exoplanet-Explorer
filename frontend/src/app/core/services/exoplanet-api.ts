import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CatalogSummary,
  ComparisonResponse,
  DiscoveryTrends,
  ExoplanetDetail,
  ExoplanetFilterState,
  ExoplanetListItem,
  MassRadiusChart,
  PagedResult,
  SyncRun,
  SyncStatusResponse
} from '../models/exoplanet.model';

@Injectable({ providedIn: 'root' })
export class ExoplanetApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  getExoplanets(filters: ExoplanetFilterState): Observable<PagedResult<ExoplanetListItem>> {
    const params = this.buildFilterParams(filters, true);
    return this.http.get<PagedResult<ExoplanetListItem>>(`${this.baseUrl}/exoplanets`, { params });
  }

  getExoplanetById(id: number): Observable<ExoplanetDetail> {
    return this.http.get<ExoplanetDetail>(`${this.baseUrl}/exoplanets/${id}`);
  }

  compareExoplanets(ids: number[]): Observable<ComparisonResponse> {
    const params = new HttpParams().set('ids', ids.join(','));
    return this.http.get<ComparisonResponse>(`${this.baseUrl}/exoplanets/compare`, { params });
  }

  getMassRadiusChart(filters: ExoplanetFilterState): Observable<MassRadiusChart> {
    const params = this.buildFilterParams(filters, false);
    return this.http.get<MassRadiusChart>(`${this.baseUrl}/charts/mass-radius`, { params });
  }

  getDiscoveryTrends(filters: ExoplanetFilterState): Observable<DiscoveryTrends> {
    const params = this.buildFilterParams(filters, false);
    return this.http.get<DiscoveryTrends>(`${this.baseUrl}/charts/discovery-trends`, { params });
  }

  getSummary(): Observable<CatalogSummary> {
    return this.http.get<CatalogSummary>(`${this.baseUrl}/stats/summary`);
  }

  getSyncStatus(): Observable<SyncStatusResponse> {
    return this.http.get<SyncStatusResponse>(`${this.baseUrl}/sync/status`);
  }

  triggerSync(): Observable<SyncRun> {
    return this.http.post<SyncRun>(`${this.baseUrl}/sync/trigger`, null);
  }

  private buildFilterParams(filters: ExoplanetFilterState, includePaging: boolean): HttpParams {
    let params = new HttpParams();

    const search = filters.search.trim();
    if (search) params = params.set('search', search);
    if (filters.minMass !== null && Number.isFinite(filters.minMass)) {
      params = params.set('minMass', filters.minMass);
    }
    if (filters.maxMass !== null && Number.isFinite(filters.maxMass)) {
      params = params.set('maxMass', filters.maxMass);
    }
    if (filters.minRadius !== null && Number.isFinite(filters.minRadius)) {
      params = params.set('minRadius', filters.minRadius);
    }
    if (filters.maxRadius !== null && Number.isFinite(filters.maxRadius)) {
      params = params.set('maxRadius', filters.maxRadius);
    }
    if (filters.minPeriod !== null && Number.isFinite(filters.minPeriod)) {
      params = params.set('minPeriod', filters.minPeriod);
    }
    if (filters.maxPeriod !== null && Number.isFinite(filters.maxPeriod)) {
      params = params.set('maxPeriod', filters.maxPeriod);
    }
    if (filters.discoveryMethod) {
      params = params.set('discoveryMethod', filters.discoveryMethod);
    }
    if (filters.habitabilityStatus) {
      params = params.set('habitabilityStatus', filters.habitabilityStatus);
    }
    if (filters.rockyOnly) {
      params = params.set('rockyOnly', 'true');
    }

    if (includePaging) {
      params = params
        .set('sortBy', filters.sortBy)
        .set('sortDir', filters.sortDir)
        .set('page', filters.page)
        .set('pageSize', filters.pageSize);
    }

    return params;
  }
}
