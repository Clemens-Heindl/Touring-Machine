import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tour } from '../models/tour.model';

@Injectable({
  providedIn: 'root'
})
export class TourService {
  private apiUrl = '/api/tours';
  private http = inject(HttpClient);

  getTours(): Observable<Tour[]> {
    return this.http.get<Tour[]>(this.apiUrl);
  }

  searchTours(query: string): Observable<Tour[]> {
    const params = new HttpParams().set('q', query);
    return this.http.get<Tour[]>(`${this.apiUrl}/search`, { params });
  }

  createTour(tour: Partial<Tour>): Observable<Tour> {
    return this.http.post<Tour>(this.apiUrl, tour);
  }

  updateTour(id: number, tour: Partial<Tour>): Observable<Tour> {
    return this.http.put<Tour>(`${this.apiUrl}/${id}`, tour);
  }

  deleteTour(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  createOrUpdateTour(tour: Partial<Tour>): Observable<Tour> {
    if (tour.id) {
      return this.updateTour(tour.id, tour);
    } else {
      return this.createTour(tour);
    }
  }
}
