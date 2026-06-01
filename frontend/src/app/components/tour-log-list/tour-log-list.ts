import { Component } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';
import { CommonModule } from '@angular/common';
import { TourLogFormComponent } from '../tour-log-form/tour-log-form.component';
import { TourLogService } from '../../services/tour-log.service';
import { TourLog } from '../../models/tour-log.model';

@Component({
  selector: 'app-tour-log-list',
  standalone: true,
  imports: [CommonModule, TourLogFormComponent],
  templateUrl: './tour-log-list.html',
  styleUrls: ['./tour-log-list.css']
})
export class TourLogListComponent {
  selectedTour = this.tourState.selectedTour$;

  constructor(
    private tourState: TourStateService,
    private logService: TourLogService
  ) { }

  deleteLog(log: TourLog, event: MouseEvent) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this log?')) {
      this.logService.deleteTourLog(log.id).subscribe(() => {
        this.tourState.deleteTourLog(log.tourId, log.id);
      });
    }
  }
}

