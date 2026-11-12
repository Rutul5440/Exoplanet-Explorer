export type HabitabilityStatus = 'Conservative' | 'Optimistic' | 'Outside' | 'Unknown';

export interface ExoplanetListItem {
  id: number;
  planetName: string;
  hostName: string;
  discoveryMethod: string;
  discoveryYear: number | null;
  massEarth: number | null;
  radiusEarth: number | null;
  orbitalPeriodDays: number | null;
  semiMajorAxisAu: number | null;
  semiMajorAxisDerived: boolean;
  equilibriumTempK: number | null;
  insolationEarth: number | null;
  habitabilityStatus: HabitabilityStatus;
  isRockyCandidate: boolean;
  earthSimilarityScore: number | null;
}

export interface ExoplanetDetail extends ExoplanetListItem {
  stellarLuminosityLogSolar: number | null;
  stellarLuminositySolar: number | null;
  luminosityDerived: boolean;
  stellarTeffK: number | null;
  stellarRadiusSolar: number | null;
  stellarMassSolar: number | null;
  distanceParsecs: number | null;
  hzInnerConservativeAu: number | null;
  hzOuterConservativeAu: number | null;
  hzInnerOptimisticAu: number | null;
  hzOuterOptimisticAu: number | null;
  lastSyncedUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ComparisonResponse {
  earthReference: ExoplanetDetail;
  planets: ExoplanetDetail[];
}

export interface MassRadiusPoint {
  id: number;
  planetName: string;
  hostName: string;
  massEarth: number;
  radiusEarth: number;
  orbitalPeriodDays: number | null;
  discoveryMethod: string;
  habitabilityStatus: HabitabilityStatus;
  isRockyCandidate: boolean;
  earthSimilarityScore: number | null;
}

export interface MassRadiusChart {
  totalMatching: number;
  returnedCount: number;
  points: MassRadiusPoint[];
}

export interface DiscoveryTrendPoint {
  year: number;
  discoveryMethod: string;
  count: number;
}

export interface DiscoveryTrends {
  methods: string[];
  points: DiscoveryTrendPoint[];
}

export interface CatalogSummary {
  totalExoplanets: number;
  conservativeHzCount: number;
  optimisticHzCount: number;
  rockyHzCount: number;
  distinctHostStars: number;
  latestDiscoveryYear: number;
  discoveryMethods: string[];
  lastSyncedUtc: string | null;
}

export interface SyncRun {
  id: number;
  startedUtc: string;
  completedUtc: string | null;
  triggerType: string;
  status: string;
  fetchedCount: number;
  insertedCount: number;
  updatedCount: number;
  unchangedCount: number;
  errorMessage: string | null;
}

export interface SyncStatusResponse {
  isSyncRunning: boolean;
  latestRun: SyncRun | null;
  recentRuns: SyncRun[];
}

export interface ExoplanetFilterState {
  search: string;
  minMass: number | null;
  maxMass: number | null;
  minRadius: number | null;
  maxRadius: number | null;
  minPeriod: number | null;
  maxPeriod: number | null;
  discoveryMethod: string;
  habitabilityStatus: '' | HabitabilityStatus;
  rockyOnly: boolean;
  sortBy: string;
  sortDir: 'asc' | 'desc';
  page: number;
  pageSize: number;
}

export function createDefaultFilterState(): ExoplanetFilterState {
  return {
    search: '',
    minMass: null,
    maxMass: null,
    minRadius: null,
    maxRadius: null,
    minPeriod: null,
    maxPeriod: null,
    discoveryMethod: '',
    habitabilityStatus: '',
    rockyOnly: false,
    sortBy: 'similarity',
    sortDir: 'desc',
    page: 1,
    pageSize: 25
  };
}

