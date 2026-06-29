import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RouteResult } from '../models/route-result.model';

@Injectable({ providedIn: 'root' })
export class RouteService {
  private readonly http = inject(HttpClient);

  getRoute(from: string, to: string, transportType: string): Observable<RouteResult> {
    const params = new HttpParams()
      .set('from', from)
      .set('to', to)
      .set('transportType', transportType);
    return this.http.get<RouteResult>('/api/route', { params });
  }
}
