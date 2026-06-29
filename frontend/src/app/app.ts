import { Component, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Header } from './components/header/header';
import { Navbar } from './components/navbar/navbar';
import { TourListComponent } from './components/tour-list/tour-list';
import { TourDetailsComponent } from './components/tour-details/tour-details';
import { TourLogListComponent } from './components/tour-log-list/tour-log-list';
import { UserStateService } from './services/user-state.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterModule,
    Header,
    Navbar,
    TourListComponent,
    TourDetailsComponent,
    TourLogListComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {
  title = 'frontend';
  private readonly userState = inject(UserStateService);
  private readonly router = inject(Router);

  get isAuthenticated() {
    return this.userState.isAuthenticated$();
  }

  get isTourRoute() {
    return this.router.url === '/' || this.router.url === '/tours';
  }
}
