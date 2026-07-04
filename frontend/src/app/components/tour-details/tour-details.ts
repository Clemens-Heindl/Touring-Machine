import { Component, inject } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';
import { ImageService } from '../../services/image.service';
import { TourService } from '../../services/tour.service';
import { downloadBlob } from '../../utils/download';
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
  private tourService = inject(TourService);

  readonly selectedTour = this.tourState.selectedTour$;
  downloadingReport = false;

  imageSrc(fileName?: string): string {
    return this.imageService.imageUrl(fileName);
  }

  downloadReport(tourId: number): void {
    this.downloadingReport = true;
    this.tourService.getTourReport(tourId).subscribe({
      next: blob => {
        downloadBlob(blob, `tour-${tourId}-report.pdf`);
        this.downloadingReport = false;
      },
      error: () => {
        this.downloadingReport = false;
        this.tourState.setApiStatus('Could not generate the PDF report.');
      }
    });
  }
}

