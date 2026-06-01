import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './components/header/header';
import { Navbar } from './components/navbar/navbar';
import { TourListComponent } from './components/tour-list/tour-list';
import { TourDetailsComponent } from './components/tour-details/tour-details';
import { TourLogListComponent } from './components/tour-log-list/tour-log-list';
import { TourFormComponent } from './components/tour-form/tour-form.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    Header,
    Navbar,
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
