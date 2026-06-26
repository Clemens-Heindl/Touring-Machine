import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TourStateService } from '../../services/tour-state.service';
import { LoginComponent } from '../login/login.component';
import { UserStateService } from '../../services/user-state.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule, LoginComponent],
  templateUrl: './header.html',
  styleUrls: ['./header.css'],
})
export class Header {
  tourState = inject(TourStateService);
  userState = inject(UserStateService);
  showAuthPanel = false;
  authMode: 'login' | 'register' = 'login';

  get isLoggedIn() {
    return this.userState.isAuthenticated$();
  }

  updateSearch(term: string) {
    this.tourState.setSearchTerm(term);
  }

  openLogin() {
    this.authMode = 'login';
    this.showAuthPanel = true;
    this.userState.clearError();
  }

  openRegister() {
    this.authMode = 'register';
    this.showAuthPanel = true;
    this.userState.clearError();
  }

  logout() {
    this.userState.logout();
    this.showAuthPanel = false;
  }
}
