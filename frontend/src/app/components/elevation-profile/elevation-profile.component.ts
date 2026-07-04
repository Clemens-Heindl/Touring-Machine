import { Component, computed, input } from '@angular/core';

interface Sample {
  distanceKm: number;
  elevationM: number;
}

interface Profile {
  ascentM: number;
  descentM: number;
  minM: number;
  maxM: number;
  distanceKm: number;
  linePath: string;
  areaPath: string;
  gridLines: { y: number; value: number }[];
  baselineY: number;
  width: number;
  height: number;
}

/**
 * Renders an elevation profile (elevation vs. distance) for a route. It parses
 * the ORS 3D GeoJSON directly, so it works from a live preview or a stored
 * route. Reusable and self-contained.
 */
@Component({
  selector: 'app-elevation-profile',
  standalone: true,
  imports: [],
  templateUrl: './elevation-profile.component.html',
  styleUrls: ['./elevation-profile.component.css']
})
export class ElevationProfileComponent {
  readonly routeGeoJson = input<string | null>(null);

  private readonly width = 640;
  private readonly height = 200;

  readonly profile = computed<Profile | null>(() => this.buildProfile(this.routeGeoJson()));

  private buildProfile(geoJson: string | null): Profile | null {
    const samples = this.extractSamples(geoJson);
    if (samples.length < 2) {
      return null;
    }

    let ascent = 0;
    let descent = 0;
    let min = Infinity;
    let max = -Infinity;
    for (let i = 0; i < samples.length; i++) {
      const ele = samples[i].elevationM;
      min = Math.min(min, ele);
      max = Math.max(max, ele);
      if (i > 0) {
        const delta = ele - samples[i - 1].elevationM;
        if (delta > 0) ascent += delta;
        else descent += -delta;
      }
    }

    const padLeft = 42;
    const padRight = 14;
    const padTop = 14;
    const padBottom = 26;
    const innerW = this.width - padLeft - padRight;
    const innerH = this.height - padTop - padBottom;
    const baselineY = padTop + innerH;

    const totalDist = samples[samples.length - 1].distanceKm || 1;
    const range = max - min || 1;

    const pts = samples.map(s => ({
      x: padLeft + (s.distanceKm / totalDist) * innerW,
      y: padTop + innerH - ((s.elevationM - min) / range) * innerH
    }));

    const linePath = pts.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x.toFixed(1)} ${p.y.toFixed(1)}`).join(' ');
    const areaPath =
      `M ${pts[0].x.toFixed(1)} ${baselineY} ` +
      pts.map(p => `L ${p.x.toFixed(1)} ${p.y.toFixed(1)}`).join(' ') +
      ` L ${pts[pts.length - 1].x.toFixed(1)} ${baselineY} Z`;

    const gridSteps = 3;
    const gridLines = Array.from({ length: gridSteps + 1 }, (_, i) => {
      const value = min + (range / gridSteps) * i;
      return { y: baselineY - ((value - min) / range) * innerH, value: Math.round(value) };
    });

    return {
      ascentM: Math.round(ascent),
      descentM: Math.round(descent),
      minM: Math.round(min),
      maxM: Math.round(max),
      distanceKm: Math.round(totalDist * 10) / 10,
      linePath,
      areaPath,
      gridLines,
      baselineY,
      width: this.width,
      height: this.height
    };
  }

  private extractSamples(geoJson: string | null): Sample[] {
    if (!geoJson) return [];

    let parsed: unknown;
    try {
      parsed = JSON.parse(geoJson);
    } catch {
      return [];
    }

    const coordinates = this.findCoordinates(parsed);
    if (!coordinates) return [];

    const samples: Sample[] = [];
    let cumulative = 0;
    let prev: number[] | null = null;

    for (const coord of coordinates) {
      if (!Array.isArray(coord) || coord.length < 3) continue;
      const [lon, lat, ele] = coord as number[];
      if (prev) {
        cumulative += this.haversineKm(prev[1], prev[0], lat, lon);
      }
      samples.push({ distanceKm: cumulative, elevationM: ele });
      prev = coord as number[];
    }

    return samples;
  }

  private findCoordinates(geo: any): number[][] | null {
    const geometry = geo?.features?.[0]?.geometry ?? geo?.geometry ?? geo;
    const coords = geometry?.coordinates;
    return Array.isArray(coords) ? coords : null;
  }

  private haversineKm(lat1: number, lon1: number, lat2: number, lon2: number): number {
    const r = 6371;
    const dLat = ((lat2 - lat1) * Math.PI) / 180;
    const dLon = ((lon2 - lon1) * Math.PI) / 180;
    const a =
      Math.sin(dLat / 2) ** 2 +
      Math.cos((lat1 * Math.PI) / 180) * Math.cos((lat2 * Math.PI) / 180) * Math.sin(dLon / 2) ** 2;
    return r * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  }
}
