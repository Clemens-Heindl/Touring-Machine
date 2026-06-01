import { Component, OnInit } from '@angular/core';
import { TourService } from '../../services/tour.service';
import { TourStateService } from '../../services/tour-state.service';
import { CommonModule } from '@angular/common';
import { Tour } from '../../models/tour.model';
import { TourFormComponent } from '../tour-form/tour-form.component';

@Component({
  selector: 'app-tour-list',
  standalone: true,
  imports: [CommonModule, TourFormComponent],
  templateUrl: './tour-list.html',
  styleUrls: ['./tour-list.css']
})
export class TourListComponent implements OnInit {
  tours = this.tourState.tours$;
  selectedTour = this.tourState.selectedTour$;

  constructor(
    private tourService: TourService,
    public tourState: TourStateService
  ) { }

  ngOnInit(): void {
    this.tourService.getTours().subscribe(tours => {
      this.tourState.setTours(tours);
    });
  }

  selectTour(tour: Tour) {
    this.tourState.setSelectedTour(tour);
  }

  deleteTour(tourId: number, event: MouseEvent) {
    event.stopPropagation(); // Prevent tour selection
    if (confirm('Are you sure you want to delete this tour?')) {
      this.tourService.deleteTour(tourId).subscribe(() => {
        this.tourState.deleteTour(tourId);
      });
    }
  }
}

