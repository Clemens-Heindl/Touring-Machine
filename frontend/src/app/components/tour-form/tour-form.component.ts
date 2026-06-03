import { Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Tour, TourFormValue, TransportType } from '../../models/tour.model';

@Component({
  selector: 'app-tour-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './tour-form.component.html',
  styleUrls: ['./tour-form.component.css']
})
export class TourFormComponent {
  tour = input<Tour | null>(null);
  saveTour = output<TourFormValue>();
  cancelEdit = output<void>();

  submitted = false;
  readonly transportTypes: TransportType[] = ['Bike', 'Hike', 'Running', 'Vacation'];
  private readonly durationPattern = /^([0-9]{1,2}):[0-5][0-9](:[0-5][0-9])?$/;
  private fb = inject(FormBuilder);

  tourForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.maxLength(500)]],
    from: ['', [Validators.required, Validators.maxLength(100)]],
    to: ['', [Validators.required, Validators.maxLength(100)]],
    transportType: ['Bike', Validators.required],
    distance: [1, [Validators.required, Validators.min(0.1)]],
    estimatedTime: ['01:00:00', [Validators.required, Validators.pattern(this.durationPattern)]],
    routeInformation: ['', [Validators.required, Validators.maxLength(500)]],
    imageUrl: ['', Validators.required]
  });

  constructor() {
    effect(() => {
      this.tour();
      this.resetForm();
    });
  }

  get isEditing(): boolean {
    return this.tour() !== null;
  }

  onSubmit() {
    this.submitted = true;

    if (this.tourForm.invalid) {
      this.tourForm.markAllAsTouched();
      return;
    }

    const formValue = this.tourForm.getRawValue() as TourFormValue;
    this.saveTour.emit({
      ...formValue,
      distance: Number(formValue.distance),
      estimatedTime: this.ensureSeconds(formValue.estimatedTime)
    });

    if (!this.isEditing) {
      this.resetForm();
    }
  }

  cancel() {
    this.cancelEdit.emit();
    this.resetForm();
  }

  hasError(controlName: string): boolean {
    const control = this.tourForm.get(controlName);

    return !!control && control.invalid && this.submitted;
  }

  private resetForm() {
    this.submitted = false;

    const tour = this.tour();
    if (tour) {
      this.tourForm.reset({
        name: tour.name,
        description: tour.description,
        from: tour.from,
        to: tour.to,
        transportType: tour.transportType,
        distance: tour.distance,
        estimatedTime: tour.estimatedTime,
        routeInformation: tour.routeInformation,
        imageUrl: tour.imageUrl
      });
      return;
    }

    this.tourForm.reset({
      name: '',
      description: '',
      from: '',
      to: '',
      transportType: 'Bike',
      distance: 1,
      estimatedTime: '01:00:00',
      routeInformation: '',
      imageUrl: ''
    });
  }

  private ensureSeconds(duration: string): string {
    return duration.split(':').length === 2 ? `${duration}:00` : duration;
  }
}
