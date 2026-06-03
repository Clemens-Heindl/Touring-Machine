import { Component } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';


@Component({
  selector: 'app-tour-details',
  standalone: true,
  imports: [],
  templateUrl: './tour-details.html',
  styleUrls: ['./tour-details.css']
})
export class TourDetailsComponent {
  get selectedTour() {
    return this.tourState.selectedTour$;
  }

  constructor(public tourState: TourStateService) { }
}

