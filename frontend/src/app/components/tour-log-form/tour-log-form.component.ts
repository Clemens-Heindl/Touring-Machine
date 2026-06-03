import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { TourLog, TourLogFormValue } from '../../models/tour-log.model';

@Component({
  selector: 'app-tour-log-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './tour-log-form.component.html',
  styleUrls: ['./tour-log-form.component.css']
})
export class TourLogFormComponent implements OnChanges {
  @Input() log: TourLog | null = null;
  @Output() saveLog = new EventEmitter<TourLogFormValue>();
  @Output() cancelEdit = new EventEmitter<void>();

  logForm: FormGroup;
  submitted = false;
  private readonly durationPattern = /^([0-9]{1,2}):[0-5][0-9](:[0-5][0-9])?$/;

  constructor(private fb: FormBuilder) {
    this.logForm = this.fb.group({
      dateTime: [this.toInputDateTime(new Date().toISOString()), Validators.required],
      comment: ['', [Validators.required, Validators.maxLength(500)]],
      difficulty: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
      totalDistance: [1, [Validators.required, Validators.min(0.1)]],
      totalTime: ['01:00:00', [Validators.required, Validators.pattern(this.durationPattern)]],
      rating: [3, [Validators.required, Validators.min(1), Validators.max(5)]]
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['log']) {
      this.resetForm();
    }
  }

  get isEditing(): boolean {
    return this.log !== null;
  }

  onSubmit() {
    this.submitted = true;

    if (this.logForm.invalid) {
      this.logForm.markAllAsTouched();
      return;
    }

    const formValue = this.logForm.getRawValue() as TourLogFormValue;
    this.saveLog.emit({
      ...formValue,
      dateTime: new Date(formValue.dateTime).toISOString(),
      difficulty: Number(formValue.difficulty),
      totalDistance: Number(formValue.totalDistance),
      totalTime: this.ensureSeconds(formValue.totalTime),
      rating: Number(formValue.rating)
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
    const control = this.logForm.get(controlName);

    return !!control && control.invalid && this.submitted;
  }

  private resetForm() {
    this.submitted = false;

    if (this.log) {
      this.logForm.reset({
        dateTime: this.toInputDateTime(this.log.dateTime),
        comment: this.log.comment,
        difficulty: this.log.difficulty,
        totalDistance: this.log.totalDistance,
        totalTime: this.log.totalTime,
        rating: this.log.rating
      });
      return;
    }

    this.logForm.reset({
      dateTime: this.toInputDateTime(new Date().toISOString()),
      comment: '',
      difficulty: 3,
      totalDistance: 1,
      totalTime: '01:00:00',
      rating: 3
    });
  }

  private toInputDateTime(value: string): string {
    const parsedDate = new Date(value);

    if (Number.isNaN(parsedDate.getTime())) {
      return this.toInputDateTime(new Date().toISOString());
    }

    const localDate = new Date(parsedDate.getTime() - parsedDate.getTimezoneOffset() * 60000);
    return localDate.toISOString().slice(0, 16);
  }

  private ensureSeconds(duration: string): string {
    return duration.split(':').length === 2 ? `${duration}:00` : duration;
  }
}
