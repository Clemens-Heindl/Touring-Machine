import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TourLog } from '../models/tour-log.model';

@Injectable({
  providedIn: 'root'
})
export class TourLogService {
  private apiUrl = '/api';
  private http = inject(HttpClient);

  createTourLog(tourId: number, log: Partial<TourLog>): Observable<TourLog> {
    return this.http.post<TourLog>(`${this.apiUrl}/tours/${tourId}/logs`, log);
  }

  updateTourLog(logId: number, log: Partial<TourLog>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/logs/${logId}`, log);
  }

  deleteTourLog(logId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/logs/${logId}`);
  }
}
