import { Component, inject } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';
import { MapComponent } from '../map/map.component';


@Component({
  selector: 'app-tour-details',
  standalone: true,
  imports: [MapComponent],
  templateUrl: './tour-details.html',
  styleUrls: ['./tour-details.css']
})
export class TourDetailsComponent {
  tourState = inject(TourStateService);

  readonly selectedTour = this.tourState.selectedTour$;
}

