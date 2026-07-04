import {
  Component,
  Input,
  OnChanges,
  AfterViewInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  SimpleChanges
} from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() routeGeoJson: string | null = null;
  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;

  private map: L.Map | null = null;
  private routeLayer: L.GeoJSON | null = null;
  private readonly storageKey = 'touringMachine.mapView';
  private restoredView = false;

  ngAfterViewInit(): void {
    this.map = L.map(this.mapContainer.nativeElement).setView([50, 10], 4);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19
    }).addTo(this.map);

    // Restore saved view (center + zoom) if available.
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (raw) {
        const parsed = JSON.parse(raw) as { lat: number; lng: number; zoom: number } | null;
        if (parsed && typeof parsed.lat === 'number' && typeof parsed.lng === 'number' && typeof parsed.zoom === 'number') {
          this.map.setView([parsed.lat, parsed.lng], parsed.zoom);
          this.restoredView = true;
        }
      }
    } catch {
      // ignore malformed storage
    }

    // Ensure Leaflet has correct size before drawing to avoid reflows.
    setTimeout(() => this.map!.invalidateSize(), 0);

    // Persist view on user interactions so it can be restored after reload.
    this.map.on('moveend', () => this.persistView());
    this.map.on('zoomend', () => this.persistView());

    if (this.routeGeoJson) {
      this.drawRoute(this.routeGeoJson);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['routeGeoJson'] && this.map) {
      if (this.routeLayer) {
        this.map.removeLayer(this.routeLayer);
        this.routeLayer = null;
      }
      if (this.routeGeoJson) {
        this.drawRoute(this.routeGeoJson);
      }
    }
  }

  ngOnDestroy(): void {
    this.map?.remove();
    this.map = null;
  }

  private drawRoute(geoJsonString: string): void {
    try {
      const geoJson = JSON.parse(geoJsonString);
      this.routeLayer = L.geoJSON(geoJson, {
        style: { color: '#2563eb', weight: 4, opacity: 0.85 }
      }).addTo(this.map!);
      const bounds = this.routeLayer.getBounds();
      if (bounds.isValid()) {
        if (!this.restoredView) {
          this.map!.fitBounds(bounds, { padding: [20, 20] });
          // After fitting to bounds, persist the new view so reload restores it.
          this.persistView();
        } else {
          // Clear the flag so subsequent route updates may fit bounds again
          // if needed.
          this.restoredView = false;
        }
      }
    } catch {
      console.error('MapComponent: failed to parse routeGeoJson');
    }
  }

  private persistView(): void {
    if (!this.map) return;
    try {
      const c = this.map.getCenter();
      const z = this.map.getZoom();
      localStorage.setItem(this.storageKey, JSON.stringify({ lat: c.lat, lng: c.lng, zoom: z }));
    } catch {
      // ignore storage errors
    }
  }
}
