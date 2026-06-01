import { Component } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tour-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tour-details.html',
  styleUrls: ['./tour-details.css']
})
export class TourDetailsComponent {
  get selectedTour() {
    return this.tourState.selectedTour$;
  }

  constructor(private tourState: TourStateService) { }
}

