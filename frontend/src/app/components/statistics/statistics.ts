import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { StatisticsService } from '../../services/statistics.service';
import { Statistics } from '../../models/statistics.model';
import { downloadBlob } from '../../utils/download';

interface TransportBar {
  name: string;
  count: number;
  widthPct: number;
  slot: number;
}

interface ActivityPoint {
  x: number;
  y: number;
  month: string;
  logCount: number;
}

interface ActivityChart {
  points: ActivityPoint[];
  linePath: string;
  areaPath: string;
  gridLines: { y: number; value: number }[];
  baselineY: number;
  width: number;
  height: number;
}

// Fixed categorical slot per transport type so a colour always follows the
// entity, never its rank in the sorted list.
const TRANSPORT_SLOTS: Record<string, number> = {
  Bike: 1,
  Hike: 2,
  Running: 3,
  Vacation: 4
};

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [],
  templateUrl: './statistics.html',
  styleUrls: ['./statistics.css']
})
export class StatisticsComponent implements OnInit {
  private readonly statisticsService = inject(StatisticsService);

  readonly stats = signal<Statistics | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  private readonly chartWidth = 640;
  private readonly chartHeight = 240;

  readonly transportBars = computed<TransportBar[]>(() => {
    const data = this.stats()?.byTransportType ?? [];
    const max = Math.max(1, ...data.map(d => d.tourCount));
    return data.map(d => ({
      name: d.transportType,
      count: d.tourCount,
      widthPct: Math.round((d.tourCount / max) * 100),
      slot: TRANSPORT_SLOTS[d.transportType] ?? 8
    }));
  });

  readonly activity = computed<ActivityChart | null>(() => {
    const data = this.stats()?.activityByMonth ?? [];
    if (data.length === 0) {
      return null;
    }

    const padLeft = 40;
    const padRight = 14;
    const padTop = 16;
    const padBottom = 30;
    const innerW = this.chartWidth - padLeft - padRight;
    const innerH = this.chartHeight - padTop - padBottom;
    const baselineY = padTop + innerH;
    const max = Math.max(1, ...data.map(d => d.logCount));
    const n = data.length;

    const points: ActivityPoint[] = data.map((d, i) => ({
      x: padLeft + (n === 1 ? innerW / 2 : (i / (n - 1)) * innerW),
      y: padTop + innerH - (d.logCount / max) * innerH,
      month: d.month,
      logCount: d.logCount
    }));

    const linePath = points
      .map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x.toFixed(1)} ${p.y.toFixed(1)}`)
      .join(' ');

    const areaPath =
      `M ${points[0].x.toFixed(1)} ${baselineY} ` +
      points.map(p => `L ${p.x.toFixed(1)} ${p.y.toFixed(1)}`).join(' ') +
      ` L ${points[points.length - 1].x.toFixed(1)} ${baselineY} Z`;

    const gridSteps = 4;
    const gridLines = Array.from({ length: gridSteps + 1 }, (_, i) => {
      const value = (max / gridSteps) * i;
      return { y: baselineY - (value / max) * innerH, value: Math.round(value) };
    });

    return { points, linePath, areaPath, gridLines, baselineY, width: this.chartWidth, height: this.chartHeight };
  });

  readonly hasData = computed(() => (this.stats()?.tourCount ?? 0) > 0);
  downloadingReport = false;

  downloadReport(): void {
    this.downloadingReport = true;
    this.statisticsService.getSummaryReport().subscribe({
      next: blob => {
        downloadBlob(blob, 'tour-summary-report.pdf');
        this.downloadingReport = false;
      },
      error: () => {
        this.downloadingReport = false;
        this.error.set('Could not generate the PDF report.');
      }
    });
  }

  ngOnInit(): void {
    this.statisticsService.getStatistics().subscribe({
      next: stats => {
        this.stats.set(stats);
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Could not load statistics. Please make sure you are signed in and the API is running.');
        this.isLoading.set(false);
      }
    });
  }

  shortMonth(month: string): string {
    // "2026-04" -> "Apr 26"
    const [year, m] = month.split('-');
    const names = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const idx = Number(m) - 1;
    return `${names[idx] ?? m} ${year.slice(2)}`;
  }
}
