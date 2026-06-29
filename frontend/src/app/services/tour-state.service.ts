import { computed, Injectable, signal } from '@angular/core';
import { Tour } from '../models/tour.model';
import { TourLog } from '../models/tour-log.model';

export interface ActiveFilters {
  transportTypes: string[];
  popularities: string[];
  childFriendliness: string[];
}

const EMPTY_FILTERS: ActiveFilters = {
  transportTypes: [],
  popularities: [],
  childFriendliness: []
};

const defaultImageUrl = 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=80';

const demoTours: Tour[] = [
  {
    id: 1,
    name: 'River Loop Ride',
    description: 'A relaxed bike loop from the harbor park to Quarry Bend with two short rest stops.',
    from: 'Harbor Park',
    to: 'Quarry Bend',
    transportType: 'Bike',
    distance: 38,
    estimatedTime: '02:10:00',
    routeInformation: 'Harbor Park -> East River Trail -> Quarry Bend',
    imageUrl: defaultImageUrl,
    logs: [
      {
        id: 11,
        tourId: 1,
        dateTime: '2026-04-26T09:30:00',
        comment: 'Sunny ride with light traffic and one coffee stop.',
        difficulty: 2,
        totalDistance: 38,
        totalTime: '02:18:00',
        rating: 4
      },
      {
        id: 12,
        tourId: 1,
        dateTime: '2026-04-12T10:00:00',
        comment: 'Wind on the last section made it slower than planned.',
        difficulty: 3,
        totalDistance: 40,
        totalTime: '02:35:00',
        rating: 3
      }
    ]
  },
  {
    id: 2,
    name: 'Alpine Day Hike',
    description: 'A scenic hike from Summit Lot to Blue Ridge with a steep middle segment.',
    from: 'Summit Lot',
    to: 'Blue Ridge',
    transportType: 'Hike',
    distance: 14,
    estimatedTime: '04:45:00',
    routeInformation: 'Summit Lot -> Pine Saddle -> Blue Ridge lookout',
    imageUrl: 'https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?auto=format&fit=crop&w=1200&q=80',
    logs: []
  }
];

@Injectable({
  providedIn: 'root'
})
export class TourStateService {
  private tours = signal<Tour[]>(demoTours);
  private selectedTourId = signal<number | null>(demoTours[0]?.id ?? null);
  private searchTerm = signal('');
  private activeFilters = signal<ActiveFilters>({ ...EMPTY_FILTERS });
  private nextLocalId = 1000;

  public readonly tours$ = this.tours.asReadonly();
  public readonly searchTerm$ = this.searchTerm.asReadonly();
  public readonly activeFilters$ = this.activeFilters.asReadonly();
  public readonly apiStatus = signal('Demo data is shown until the ASP.NET API responds.');

  public readonly hasActiveSearch$ = computed(() => {
    const term = this.searchTerm().trim();
    const f = this.activeFilters();
    return term.length > 0
      || f.transportTypes.length > 0
      || f.popularities.length > 0
      || f.childFriendliness.length > 0;
  });

  public readonly filteredTours$ = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const tokens = term.split(/\s+/).filter(t => t.length > 0);
    const f = this.activeFilters();

    return this.tours().filter(tour => {
      // --- chip filters (OR within group, AND across groups) ---
      if (f.transportTypes.length > 0 && !f.transportTypes.includes(tour.transportType)) return false;
      if (f.popularities.length > 0 && !f.popularities.includes(this.getPopularity(tour))) return false;
      if (f.childFriendliness.length > 0 && !f.childFriendliness.includes(this.getChildFriendliness(tour))) return false;

      // --- text search (all tokens must match somewhere in the haystack) ---
      if (tokens.length === 0) return true;

      const logText = tour.logs
        .map(l => `${l.comment} ${l.difficulty} ${l.rating} ${l.totalDistance} ${l.totalTime}`)
        .join(' ');

      const popularity = this.getPopularity(tour);
      const childFriendliness = this.getChildFriendliness(tour);
      const popularityAliases = this.popularityAliases(popularity);
      const childFriendlinessAliases = this.childFriendlinessAliases(childFriendliness);

      const haystack = [
        tour.name, tour.description, tour.from, tour.to,
        tour.transportType, tour.distance, tour.estimatedTime,
        tour.routeInformation, logText,
        popularity, popularityAliases,
        childFriendliness, childFriendlinessAliases
      ].join(' ').toLowerCase();

      return tokens.every(token => haystack.includes(token));
    });
  });
  public readonly selectedTour$ = computed(() => {
    const selectedId = this.selectedTourId();

    return this.tours().find(tour => tour.id === selectedId) ?? null;
  });

  setTours(tours: Tour[]) {
    const normalizedTours = tours.map(tour => this.normalizeTour(tour));
    this.tours.set(normalizedTours);
    this.selectedTourId.set(normalizedTours[0]?.id ?? null);
    this.apiStatus.set(normalizedTours.length > 0 ? 'Connected to the ASP.NET API.' : 'Connected to the API. Create your first tour.');
  }

  setSelectedTour(tour: Tour | null) {
    this.selectedTourId.set(tour?.id ?? null);
  }

  setSearchTerm(term: string) {
    this.searchTerm.set(term);
  }

  toggleTransportFilter(type: string): void {
    this.activeFilters.update(f => ({
      ...f,
      transportTypes: f.transportTypes.includes(type)
        ? f.transportTypes.filter(t => t !== type)
        : [...f.transportTypes, type]
    }));
  }

  togglePopularityFilter(value: string): void {
    this.activeFilters.update(f => ({
      ...f,
      popularities: f.popularities.includes(value)
        ? f.popularities.filter(v => v !== value)
        : [...f.popularities, value]
    }));
  }

  toggleChildFriendlinessFilter(value: string): void {
    this.activeFilters.update(f => ({
      ...f,
      childFriendliness: f.childFriendliness.includes(value)
        ? f.childFriendliness.filter(v => v !== value)
        : [...f.childFriendliness, value]
    }));
  }

  clearAllFilters(): void {
    this.searchTerm.set('');
    this.activeFilters.set({ ...EMPTY_FILTERS });
  }

  setApiStatus(message: string) {
    this.apiStatus.set(message);
  }

  addTour(tour: Partial<Tour>) {
    const normalizedTour = this.normalizeTour(tour);
    this.tours.update(current => [...current, normalizedTour]);
    this.setSelectedTour(normalizedTour);
  }

  updateTour(updatedTour: Partial<Tour> & { id: number }) {
    this.tours.update(current => current.map(tour => tour.id === updatedTour.id ? this.normalizeTour({ ...tour, ...updatedTour }) : tour));
  }

  deleteTour(tourId: number) {
    this.tours.update(current => current.filter(t => t.id !== tourId));
    this.selectedTourId.update(currentId => currentId === tourId ? (this.tours()[0]?.id ?? null) : currentId);
  }

  addTourLog(tourId: number, log: Partial<TourLog>) {
    const normalizedLog = this.normalizeLog(log, tourId);

    this.tours.update(current => current.map(tour => {
      if (tour.id === tourId) {
        return { ...tour, logs: [...tour.logs, normalizedLog] };
      }

      return tour;
    }));
  }

  updateTourLog(tourId: number, updatedLog: Partial<TourLog> & { id: number }) {
    this.tours.update(current => current.map(tour => {
      if (tour.id !== tourId) {
        return tour;
      }

      return {
        ...tour,
        logs: tour.logs.map(log => log.id === updatedLog.id ? this.normalizeLog({ ...log, ...updatedLog }, tourId) : log)
      };
    }));
  }

  deleteTourLog(tourId: number, logId: number) {
    this.tours.update(current => current.map(t => {
      if (t.id === tourId) {
        return { ...t, logs: t.logs.filter(l => l.id !== logId) };
      }

      return t;
    }));
  }

  getPopularity(tour: Tour): string {
    if (tour.logs.length === 0) {
      return 'New';
    }

    if (tour.logs.length < 3) {
      return 'Known';
    }

    return 'Popular';
  }

  getChildFriendliness(tour: Tour): string {
    if (tour.logs.length === 0) {
      return tour.distance <= 10 ? 'Likely child-friendly' : 'Unknown';
    }

    const averageDifficulty = tour.logs.reduce((sum, log) => sum + log.difficulty, 0) / tour.logs.length;
    const averageDistance = tour.logs.reduce((sum, log) => sum + log.totalDistance, 0) / tour.logs.length;

    if (averageDifficulty <= 2 && averageDistance <= 12) {
      return 'Child-friendly';
    }

    if (averageDifficulty <= 3 && averageDistance <= 25) {
      return 'Moderate';
    }

    return 'Challenging';
  }

  private normalizeTour(tour: Partial<Tour>): Tour {
    return {
      id: tour.id ?? this.createLocalId(),
      name: this.cleanText(tour.name, 'Untitled tour'),
      description: this.cleanText(tour.description, 'No description yet.'),
      from: this.cleanText(tour.from, 'Unknown start'),
      to: this.cleanText(tour.to, 'Unknown destination'),
      transportType: tour.transportType ?? 'Bike',
      distance: Number(tour.distance ?? 0),
      estimatedTime: this.cleanText(tour.estimatedTime, '00:00:00'),
      routeInformation: this.cleanText(tour.routeInformation, 'Route information will be added by OpenRouteService.'),
      imageUrl: this.cleanText(tour.imageUrl, defaultImageUrl),
      logs: (tour.logs ?? []).map(log => this.normalizeLog(log, tour.id ?? 0))
    };
  }

  private normalizeLog(log: Partial<TourLog>, tourId: number): TourLog {
    return {
      id: log.id ?? this.createLocalId(),
      tourId: log.tourId ?? tourId,
      dateTime: this.cleanText(log.dateTime, new Date().toISOString()),
      comment: this.cleanText(log.comment, 'No comment.'),
      difficulty: Number(log.difficulty ?? 3),
      totalDistance: Number(log.totalDistance ?? 0),
      totalTime: this.cleanText(log.totalTime, '00:00:00'),
      rating: Number(log.rating ?? 3)
    };
  }

  private popularityAliases(popularity: string): string {
    const map: Record<string, string> = {
      'New': 'new unvisited first',
      'Known': 'known visited',
      'Popular': 'popular frequent top'
    };
    return map[popularity] ?? '';
  }

  private childFriendlinessAliases(cf: string): string {
    const map: Record<string, string> = {
      'Child-friendly': 'easy family kids children friendly',
      'Likely child-friendly': 'easy likely family kids children',
      'Moderate': 'medium moderate average',
      'Challenging': 'hard difficult challenging tough advanced',
      'Unknown': 'unknown'
    };
    return map[cf] ?? '';
  }

  private createLocalId(): number {
    this.nextLocalId += 1;

    return this.nextLocalId;
  }

  private cleanText(value: string | null | undefined, fallback: string): string {
    const cleanedValue = value?.trim();

    return cleanedValue && cleanedValue.length > 0 ? cleanedValue : fallback;
  }
}
