import { Component, effect } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TourLogService } from '../../services/tour-log.service';
import { TourStateService } from '../../services/tour-state.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tour-log-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tour-log-form.component.html',
  styleUrls: ['./tour-log-form.component.css']
})
export class TourLogFormComponent {
  logForm: FormGroup;
  selectedTourId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private logService: TourLogService,
    public tourState: TourStateService
  ) {
    this.logForm = this.fb.group({
      dateTime: [new Date().toISOString(), Validators.required],
      comment: [''],
      difficulty: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
      totalDistance: [0, Validators.required],
      totalTime: ['00:00:00', Validators.required],
      rating: [3, [Validators.required, Validators.min(1), Validators.max(5)]]
    });

    // Use effect to watch for selected tour changes
    effect(() => {
      const tour = this.tourState.selectedTour$();
      this.selectedTourId = tour ? tour.id : null;
    });
  }

  onSubmit() {
    if (this.logForm.valid && this.selectedTourId) {
      this.logService.createTourLog(this.selectedTourId, this.logForm.value).subscribe(newLog => {
        this.tourState.addTourLog(this.selectedTourId!, newLog);
        this.logForm.reset({
          dateTime: new Date().toISOString(),
          difficulty: 3,
          rating: 3,
          totalDistance: 0,
          totalTime: '00:00:00'
        });
      });
    }
  }
}
