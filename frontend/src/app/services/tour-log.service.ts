import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TourLog } from '../models/tour-log.model';

@Injectable({
  providedIn: 'root'
})
export class TourLogService {
  private apiUrl = 'https://localhost:7125/api'; // Adjust port if necessary

  constructor(private http: HttpClient) { }

  createTourLog(tourId: number, log: Partial<TourLog>): Observable<TourLog> {
    return this.http.post<TourLog>(`${this.apiUrl}/tours/${tourId}/logs`, log);
  }

  deleteTourLog(logId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/logs/${logId}`);
  }
}
