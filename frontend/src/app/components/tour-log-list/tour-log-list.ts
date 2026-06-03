import { Component, inject } from '@angular/core';
import { TourStateService } from '../../services/tour-state.service';
import { DatePipe } from '@angular/common';
import { TourLogFormComponent } from '../tour-log-form/tour-log-form.component';
import { TourLogService } from '../../services/tour-log.service';
import { TourLog, TourLogFormValue } from '../../models/tour-log.model';

@Component({
  selector: 'app-tour-log-list',
  standalone: true,
  imports: [DatePipe, TourLogFormComponent],
  templateUrl: './tour-log-list.html',
  styleUrls: ['./tour-log-list.css']
})
export class TourLogListComponent {
  private tourState = inject(TourStateService);
  private logService = inject(TourLogService);

  readonly selectedTour = this.tourState.selectedTour$;

  showForm = false;
  editingLog: TourLog | null = null;

  startCreateLog() {
    this.editingLog = null;
    this.showForm = true;
  }

  startEditLog(log: TourLog) {
    this.editingLog = log;
    this.showForm = true;
  }

  saveLog(formValue: TourLogFormValue) {
    const tour = this.selectedTour();

    if (!tour) {
      return;
    }

    if (this.editingLog) {
      const updatedLog = { ...this.editingLog, ...formValue, tourId: tour.id };

      this.logService.updateTourLog(updatedLog.id, updatedLog).subscribe({
        next: () => {
          this.tourState.updateTourLog(tour.id, updatedLog);
          this.closeLogForm('Log updated.');
        },
        error: () => {
          this.tourState.updateTourLog(tour.id, updatedLog);
          this.closeLogForm('Log updated locally. Start the API to persist changes.');
        }
      });
      return;
    }

    const draftLog = { ...formValue, tourId: tour.id };
    this.logService.createTourLog(tour.id, draftLog).subscribe({
      next: newLog => {
        this.tourState.addTourLog(tour.id, { ...draftLog, ...newLog });
        this.closeLogForm('Log created.');
      },
      error: () => {
        this.tourState.addTourLog(tour.id, draftLog);
        this.closeLogForm('Log created locally. Start the API to persist changes.');
      }
    });
  }

  cancelLogForm() {
    this.showForm = false;
    this.editingLog = null;
  }

  deleteLog(log: TourLog, event: MouseEvent) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this log?')) {
      this.logService.deleteTourLog(log.id).subscribe({
        next: () => {
          this.tourState.deleteTourLog(log.tourId, log.id);
          this.tourState.setApiStatus('Log deleted.');
        },
        error: () => {
          this.tourState.deleteTourLog(log.tourId, log.id);
          this.tourState.setApiStatus('Log deleted locally. Start the API to persist changes.');
        }
      });
    }
  }

  private closeLogForm(message: string) {
    this.showForm = false;
    this.editingLog = null;
    this.tourState.setApiStatus(message);
  }
}

