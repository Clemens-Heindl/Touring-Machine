import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header/header.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { TourListComponent } from './components/tour-list/tour-list.component';
import { TourDetailsComponent } from './components/tour-details/tour-details.component';
import { TourLogListComponent } from './components/tour-log-list/tour-log-list.component';
import { TourFormComponent } from './components/tour-form/tour-form.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    HeaderComponent,
    NavbarComponent,
    TourListComponent,
    TourDetailsComponent,
    TourLogListComponent,
    TourFormComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {
  title = 'frontend';
}
