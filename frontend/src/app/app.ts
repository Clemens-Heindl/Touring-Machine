import { Component } from '@angular/core';
import { Header } from './components/header/header';
import { Navbar } from './components/navbar/navbar';
import { TourListComponent } from './components/tour-list/tour-list';
import { TourDetailsComponent } from './components/tour-details/tour-details';
import { TourLogListComponent } from './components/tour-log-list/tour-log-list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
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
}
