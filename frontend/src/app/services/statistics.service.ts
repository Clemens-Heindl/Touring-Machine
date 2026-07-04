import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Statistics } from '../models/statistics.model';

@Injectable({ providedIn: 'root' })
export class StatisticsService {
  private apiUrl = '/api/statistics';
  private http = inject(HttpClient);

  getStatistics(): Observable<Statistics> {
    return this.http.get<Statistics>(this.apiUrl);
  }

  getSummaryReport(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/report`, { responseType: 'blob' });
  }
}
