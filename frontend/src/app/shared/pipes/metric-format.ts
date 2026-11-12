export function formatMetric(
  value: number | null | undefined,
  decimals = 2,
  suffix = ''
): string {
  if (value === null || value === undefined || !Number.isFinite(value)) {
    return '—';
  }

  const abs = Math.abs(value);
  let formatted: string;

  if (abs > 0 && abs < 0.001) {
    formatted = value.toExponential(2);
  } else if (abs >= 1000) {
    formatted = value.toLocaleString('en-US', {
      maximumFractionDigits: Math.min(decimals, 1)
    });
  } else {
    formatted = value.toLocaleString('en-US', {
      minimumFractionDigits: 0,
      maximumFractionDigits: decimals
    });
  }

  return suffix ? `${formatted} ${suffix}` : formatted;
}

const METHOD_COLORS: Record<string, string> = {
  Transit: '#1E5BFF',
  'Radial Velocity': '#E21D2D',
  Microlensing: '#FFC20E',
  Imaging: '#22B14C',
  'Transit Timing Variations': '#38BDF8',
  'Eclipse Timing Variations': '#A78BFA',
  'Orbital Brightness Modulation': '#FB7185',
  Astrometry: '#2DD4BF'
};

export function getDiscoveryMethodColor(method: string): string {
  return METHOD_COLORS[method] ?? '#93A9D2';
}

