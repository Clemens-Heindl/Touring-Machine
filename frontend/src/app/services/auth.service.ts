import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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

  register(user: User): Observable<User> {
    return this.http.post<User>(this.apiUrl, user);
  }

  login(userId: number, password: string): Observable<boolean> {
    const params = new HttpParams()
      .set('id', userId.toString())
      .set('PasswordHash', password);

    return this.http.get<boolean>(`${this.apiUrl}/login`, { params });
  }
}
