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

  ngAfterViewInit(): void {
    this.map = L.map(this.mapContainer.nativeElement).setView([50, 10], 4);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19
    }).addTo(this.map);

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
        this.map!.fitBounds(bounds, { padding: [20, 20] });
      }
    } catch {
      console.error('MapComponent: failed to parse routeGeoJson');
    }
  }
}
