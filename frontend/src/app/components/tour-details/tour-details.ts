import { Component, inject } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';
import { ImageService } from '../../services/image.service';
import { MapComponent } from '../map/map.component';
import { ElevationProfileComponent } from '../elevation-profile/elevation-profile.component';


@Component({
  selector: 'app-tour-details',
  standalone: true,
  imports: [MapComponent, ElevationProfileComponent],
  templateUrl: './tour-details.html',
  styleUrls: ['./tour-details.css']
})
export class TourDetailsComponent {
  tourState = inject(TourStateService);
  private imageService = inject(ImageService);

  readonly selectedTour = this.tourState.selectedTour$;

  imageSrc(fileName?: string): string {
    return this.imageService.imageUrl(fileName);
  }
}

