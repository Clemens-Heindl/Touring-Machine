import { computed, Injectable, signal } from '@angular/core';
import { Tour } from '../models/tour.model';
import { TourLog } from '../models/tour-log.model';

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
  private nextLocalId = 1000;

  public readonly tours$ = this.tours.asReadonly();
  public readonly searchTerm$ = this.searchTerm.asReadonly();
  public readonly apiStatus = signal('Demo data is shown until the ASP.NET API responds.');
  public readonly filteredTours$ = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();

    if (!term) {
      return this.tours();
    }

    return this.tours().filter(tour => {
      const logText = tour.logs
        .map(log => `${log.comment} ${log.difficulty} ${log.rating} ${log.totalDistance}`)
        .join(' ');
      const haystack = `${tour.name} ${tour.description} ${tour.from} ${tour.to} ${tour.transportType} ${tour.distance} ${tour.estimatedTime} ${tour.routeInformation} ${this.getPopularity(tour)} ${this.getChildFriendliness(tour)} ${logText}`;

      return haystack.toLowerCase().includes(term);
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

  private createLocalId(): number {
    this.nextLocalId += 1;

    return this.nextLocalId;
  }

  private cleanText(value: string | null | undefined, fallback: string): string {
    const cleanedValue = value?.trim();

    return cleanedValue && cleanedValue.length > 0 ? cleanedValue : fallback;
  }
}
