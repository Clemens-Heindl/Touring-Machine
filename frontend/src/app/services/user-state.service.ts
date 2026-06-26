import { computed, inject, Injectable, signal } from '@angular/core';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserStateService {
  private currentUser = signal<User | null>(null);
  private errorMessage = signal('');

  readonly currentUser$ = this.currentUser.asReadonly();
  readonly isAuthenticated$ = computed(() => this.currentUser() !== null);
  readonly errorMessage$ = this.errorMessage.asReadonly();

  setCurrentUser(user: User | null) {
    this.currentUser.set(user);
  }

  setError(message: string) {
    this.errorMessage.set(message);
  }

  clearError() {
    this.errorMessage.set('');
  }

  logout() {
    this.currentUser.set(null);
    this.clearError();
  }
}
