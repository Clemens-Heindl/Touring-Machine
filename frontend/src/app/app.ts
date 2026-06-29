import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Header } from './components/header/header';
import { Navbar } from './components/navbar/navbar';
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
    TourDetailsComponent,
    TourLogListComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {
  title = 'frontend';
  private readonly userState = inject(UserStateService);

  get isAuthenticated() {
    return this.userState.isAuthenticated$();
  }
}
