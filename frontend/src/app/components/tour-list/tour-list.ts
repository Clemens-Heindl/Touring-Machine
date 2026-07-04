import { Component, inject, OnInit } from '@angular/core';
import { TourService } from '../../services/tour.service';
import { TourStateService } from '../../services/tour-state.service';
import { UserStateService } from '../../services/user-state.service';
import { ImageService } from '../../services/image.service';

import { Tour, TourFormValue } from '../../models/tour.model';
import { TourFormComponent } from '../tour-form/tour-form.component';
import { SearchFiltersComponent } from '../search-filters/search-filters.component';

@Component({
  selector: 'app-tour-list',
  standalone: true,
  imports: [TourFormComponent, SearchFiltersComponent],
  templateUrl: './tour-list.html',
  styleUrls: ['./tour-list.css']
})
export class TourListComponent implements OnInit {
  private tourService = inject(TourService);
  private userState = inject(UserStateService);
  private imageService = inject(ImageService);
  tourState = inject(TourStateService);

  readonly tours = this.tourState.filteredTours$;
  readonly selectedTour = this.tourState.selectedTour$;
  readonly totalTours = this.tourState.tours$;

  showForm = false;
  editingTour: Tour | null = null;

  ngOnInit(): void {
    // Only show the generic loading status when we don't already have cached tours.
    if ((this.tourState.tours$() ?? []).length === 0) {
      this.tourState.setApiStatus('Loading tours from the ASP.NET API...');
    }

    this.tourService.getTours().subscribe({
      next: tours => {
        this.tourState.setTours(tours);
      },
      error: () => {
        // Only show an explicit unreachable message when there is no cached data.
        if ((this.tourState.tours$() ?? []).length === 0) {
          this.tourState.setApiStatus('API is not reachable. You can still use the local demo data.');
        }
        // If cached data exists, keep the connected text so the UI message is
        // identical whether the content came from localStorage or the API.
      }
    });
  }

  selectTour(tour: Tour) {
    this.tourState.setSelectedTour(tour);
  }

  imageSrc(fileName?: string): string {
    return this.imageService.imageUrl(fileName);
  }

  startCreateTour() {
    this.editingTour = null;
    this.showForm = true;
  }

  startEditTour(tour: Tour, event: MouseEvent) {
    event.stopPropagation();
    this.editingTour = tour;
    this.showForm = true;
  }

  saveTour(formValue: TourFormValue) {
    const userId = this.userState.currentUser$()?.id ?? 0;

    if (this.editingTour) {
      const updatedTour = { ...this.editingTour, ...formValue };

      this.tourService.updateTour(updatedTour.id, { ...formValue, id: updatedTour.id, userId: updatedTour.userId }).subscribe({
        next: (returnedTour) => {
          this.tourState.updateTour(returnedTour);
          this.closeForm('Tour updated.');
        },
        error: () => {
          this.tourState.updateTour(updatedTour);
          this.closeForm('Tour updated locally. Start the API to persist changes.');
        }
      });
      return;
    }

    const draftTour: Partial<Tour> = { ...formValue, userId, logs: [] };
    this.tourService.createTour(draftTour).subscribe({
      next: newTour => {
        this.tourState.addTour({ ...draftTour, ...newTour, imageFileName: newTour.imageFileName ?? formValue.imageFileName });
        this.closeForm('Tour created.');
      },
      error: () => {
        this.tourState.addTour(draftTour);
        this.closeForm('Tour created locally. Start the API to persist changes.');
      }
    });
  }

  cancelForm() {
    this.showForm = false;
    this.editingTour = null;
  }

  deleteTour(tourId: number, event: MouseEvent) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this tour?')) {
      this.tourService.deleteTour(tourId).subscribe({
        next: () => {
          this.tourState.deleteTour(tourId);
          this.tourState.setApiStatus('Tour deleted.');
        },
        error: () => {
          this.tourState.deleteTour(tourId);
          this.tourState.setApiStatus('Tour deleted locally. Start the API to persist changes.');
        }
      });
    }
  }

  private closeForm(message: string) {
    this.showForm = false;
    this.editingTour = null;
    this.tourState.setApiStatus(message);
  }
}

