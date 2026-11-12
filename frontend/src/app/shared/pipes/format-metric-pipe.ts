import { Pipe, PipeTransform } from '@angular/core';
import { formatMetric } from './metric-format';

@Pipe({ name: 'formatMetric' })
export class FormatMetricPipe implements PipeTransform {
  transform(value: number | null | undefined, decimals = 2, suffix = ''): string {
    return formatMetric(value, decimals, suffix);
  }
}

