import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TourService } from '../../services/tour.service';
import { TourStateService } from '../../services/tour-state.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tour-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tour-form.component.html',
  styleUrls: ['./tour-form.component.css']
})
export class TourFormComponent {
  tourForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private tourService: TourService,
    private tourState: TourStateService
  ) {
    this.tourForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      from: ['', Validators.required],
      to: ['', Validators.required],
      transportType: ['', Validators.required],
      distance: [0],
      estimatedTime: ['00:00:00']
    });
  }

  onSubmit() {
    if (this.tourForm.valid) {
      this.tourService.createTour(this.tourForm.value).subscribe(newTour => {
        this.tourState.addTour(newTour);
        this.tourForm.reset();
      });
    }
  }
}
