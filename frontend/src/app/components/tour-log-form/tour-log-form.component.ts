import { Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { TourLog, TourLogFormValue } from '../../models/tour-log.model';

@Component({
  selector: 'app-tour-log-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './tour-log-form.component.html',
  styleUrls: ['./tour-log-form.component.css']
})
export class TourLogFormComponent {
  log = input<TourLog | null>(null);
  saveLog = output<TourLogFormValue>();
  cancelEdit = output<void>();

  submitted = false;
  private readonly durationPattern = /^([0-9]{1,2}):[0-5][0-9](:[0-5][0-9])?$/;
  private fb = inject(FormBuilder);

  logForm = this.fb.group({
    dateTime: [this.toInputDateTime(new Date().toISOString()), Validators.required],
    comment: ['', [Validators.required, Validators.maxLength(500)]],
    difficulty: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
    totalDistance: [1, [Validators.required, Validators.min(0.1)]],
    totalTime: ['01:00:00', [Validators.required, Validators.pattern(this.durationPattern)]],
    rating: [3, [Validators.required, Validators.min(1), Validators.max(5)]]
  });

  constructor() {
    effect(() => {
      this.log();
      this.resetForm();
    });
  }

  get isEditing(): boolean {
    return this.log() !== null;
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

  get difficultyValue(): number {
    return Number(this.logForm.get('difficulty')?.value ?? 0);
  }

  get ratingValue(): number {
    return Number(this.logForm.get('rating')?.value ?? 0);
  }

  setDifficulty(value: number): void {
    this.logForm.get('difficulty')?.setValue(value);
  }

  setRating(value: number): void {
    this.logForm.get('rating')?.setValue(value);
  }

  private resetForm() {
    this.submitted = false;

    const log = this.log();
    if (log) {
      this.logForm.reset({
        dateTime: this.toInputDateTime(log.dateTime),
        comment: log.comment,
        difficulty: log.difficulty,
        totalDistance: log.totalDistance,
        totalTime: log.totalTime,
        rating: log.rating
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
