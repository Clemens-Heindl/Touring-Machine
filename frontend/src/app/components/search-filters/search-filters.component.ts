import { Component, inject } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';

@Component({
  selector: 'app-search-filters',
  standalone: true,
  imports: [],
  templateUrl: './search-filters.component.html',
  styleUrls: ['./search-filters.component.css']
})
export class SearchFiltersComponent {
  readonly tourState = inject(TourStateService);

  readonly transportTypes = ['Car', 'Bike', 'Hike', 'Walk'];
  readonly popularities = ['New', 'Known', 'Popular'];
  readonly suitabilities = ['Child-friendly', 'Moderate', 'Challenging'];

  isTransportActive(type: string): boolean {
    return this.tourState.activeFilters$().transportTypes.includes(type);
  }

  isPopularityActive(value: string): boolean {
    return this.tourState.activeFilters$().popularities.includes(value);
  }

  isSuitabilityActive(value: string): boolean {
    return this.tourState.activeFilters$().childFriendliness.includes(value);
  }
}
