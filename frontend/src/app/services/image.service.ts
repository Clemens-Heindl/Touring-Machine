import { HttpClient, HttpEvent } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

const PLACEHOLDER =
  "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='240'%3E%3Crect width='400' height='240' fill='%23e2e8f0'/%3E%3Cpath d='M0 200 L120 120 L200 170 L300 90 L400 180 L400 240 L0 240 Z' fill='%2394a3b8'/%3E%3Ccircle cx='320' cy='60' r='24' fill='%23cbd5e1'/%3E%3C/svg%3E";

@Injectable({ providedIn: 'root' })
export class ImageService {
  private apiUrl = '/api/images';
  private http = inject(HttpClient);

  /** Uploads an image and reports progress; the final event carries { fileName }. */
  upload(file: File): Observable<HttpEvent<{ fileName: string }>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ fileName: string }>(this.apiUrl, form, {
      reportProgress: true,
      observe: 'events'
    });
  }

  /** Resolves a stored file name to its served URL, or a placeholder when absent. */
  imageUrl(fileName?: string | null): string {
    return fileName ? `${this.apiUrl}/${fileName}` : PLACEHOLDER;
  }
}
