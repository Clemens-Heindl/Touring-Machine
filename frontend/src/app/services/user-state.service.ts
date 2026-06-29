import { computed, Injectable, signal } from '@angular/core';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserStateService {
  private readonly storageKey = 'touring-machine-user';
  private currentUser = signal<User | null>(this.readStoredUser());
  private errorMessage = signal('');

  readonly currentUser$ = this.currentUser.asReadonly();
  readonly isAuthenticated$ = computed(() => this.currentUser() !== null);
  readonly errorMessage$ = this.errorMessage.asReadonly();

  setCurrentUser(user: User | null) {
    this.currentUser.set(user);
    if (user) {
      localStorage.setItem(this.storageKey, JSON.stringify(user));
    } else {
      localStorage.removeItem(this.storageKey);
    }
  }

  setError(message: string) {
    this.errorMessage.set(message);
  }

  clearError() {
    this.errorMessage.set('');
  }

  logout() {
    this.currentUser.set(null);
    localStorage.removeItem(this.storageKey);
    this.clearError();
  }

  private readStoredUser(): User | null {
    if (typeof window === 'undefined' || typeof localStorage === 'undefined') {
      return null;
    }

    const storedUser = localStorage.getItem(this.storageKey);
    if (!storedUser) {
      return null;
    }

    try {
      return JSON.parse(storedUser) as User;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
