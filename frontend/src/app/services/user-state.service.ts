import { computed, Injectable, signal } from '@angular/core';
import { User } from '../models/user.model';
import { AuthResponse } from '../models/auth-response.model';

@Injectable({
  providedIn: 'root'
})
export class UserStateService {
  private readonly userKey = 'touring-machine-user';
  private readonly tokenKey = 'touring-machine-token';

  private currentUser = signal<User | null>(this.readStoredUser());
  private token = signal<string | null>(this.readStoredToken());
  private errorMessage = signal('');

  readonly currentUser$ = this.currentUser.asReadonly();
  readonly token$ = this.token.asReadonly();
  readonly isAuthenticated$ = computed(() => this.token() !== null && this.currentUser() !== null);
  readonly errorMessage$ = this.errorMessage.asReadonly();

  /** Persists the token + user returned by login/register. */
  setSession(auth: AuthResponse) {
    this.currentUser.set(auth.user);
    this.token.set(auth.token);
    localStorage.setItem(this.userKey, JSON.stringify(auth.user));
    localStorage.setItem(this.tokenKey, auth.token);
  }

  setError(message: string) {
    this.errorMessage.set(message);
  }

  clearError() {
    this.errorMessage.set('');
  }

  logout() {
    this.currentUser.set(null);
    this.token.set(null);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.tokenKey);
    this.clearError();
  }

  private readStoredUser(): User | null {
    const stored = this.readStorage(this.userKey);
    if (!stored) {
      return null;
    }

    try {
      return JSON.parse(stored) as User;
    } catch {
      localStorage.removeItem(this.userKey);
      return null;
    }
  }

  private readStoredToken(): string | null {
    return this.readStorage(this.tokenKey);
  }

  private readStorage(key: string): string | null {
    if (typeof window === 'undefined' || typeof localStorage === 'undefined') {
      return null;
    }
    return localStorage.getItem(key);
  }
}
