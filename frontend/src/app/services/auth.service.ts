import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/users';
  private http = inject(HttpClient);

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  private async hashPassword(password: string): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(password);
    const digest = await crypto.subtle.digest('SHA-256', data);

    return Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('');
  }

  register(user: User): Observable<User> {
    return from(this.hashPassword(user.passwordHash)).pipe(
      switchMap(passwordHash => {
        const secureUser = { ...user, passwordHash };
        return this.http.post<User>(this.apiUrl, secureUser);
      })
    );
  }

  login(email: string, password: string): Observable<boolean> {
    return from(this.hashPassword(password)).pipe(
      switchMap(passwordHash => {
        const params = new HttpParams()
          .set('email', email)
          .set('passwordHash', passwordHash);

        return this.http.get<boolean>(`${this.apiUrl}/login`, { params });
      })
    );
  }
}
