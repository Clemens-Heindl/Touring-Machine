import { Injectable, signal } from '@angular/core';
import { Tour } from '../models/tour.model';
import { TourLog } from '../models/tour-log.model';

@Injectable({
  providedIn: 'root'
})
export class TourStateService {
  private tours = signal<Tour[]>([]);
  private selectedTour = signal<Tour | null>(null);

  public readonly tours$ = this.tours.asReadonly();
  public readonly selectedTour$ = this.selectedTour.asReadonly();

  constructor() { }

  setTours(tours: Tour[]) {
    this.tours.set(tours);
  }

  setSelectedTour(tour: Tour | null) {
    this.selectedTour.set(tour);
  }

  addTour(tour: Tour) {
    this.tours.update(current => [...current, tour]);
  }

  updateTour(updatedTour: Tour) {
    this.tours.update(current => current.map(t => t.id === updatedTour.id ? updatedTour : t));
    if (this.selectedTour$()?.id === updatedTour.id) {
      this.setSelectedTour(updatedTour);
    }
  }

  deleteTour(tourId: number) {
    this.tours.update(current => current.filter(t => t.id !== tourId));
    if (this.selectedTour$()?.id === tourId) {
      this.setSelectedTour(null);
    }
  }

  addTourLog(tourId: number, log: TourLog) {
      this.tours.update(current => current.map(t => {
          if (t.id === tourId) {
              return { ...t, logs: [...t.logs, log] };
          }
          return t;
      }));
      // Refresh selected tour if it's the one being updated
      if (this.selectedTour$()?.id === tourId) {
        this.setSelectedTour(this.tours$().find(t => t.id === tourId) || null);
      }
  }

  deleteTourLog(tourId: number, logId: number) {
      this.tours.update(current => current.map(t => {
          if (t.id === tourId) {
              return { ...t, logs: t.logs.filter(l => l.id !== logId) };
          }
          return t;
      }));
       if (this.selectedTour$()?.id === tourId) {
        this.setSelectedTour(this.tours$().find(t => t.id === tourId) || null);
      }
  }
}
