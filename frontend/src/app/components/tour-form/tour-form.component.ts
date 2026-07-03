import { Component, DestroyRef, effect, inject, input, OnInit, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, merge } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, filter, map, switchMap } from 'rxjs/operators';
import { Tour, TourFormValue, TransportType } from '../../models/tour.model';
import { RouteService } from '../../services/route.service';
import { MapComponent } from '../map/map.component';
import { ImageUploadComponent } from '../image-upload/image-upload.component';

@Component({
  selector: 'app-tour-form',
  standalone: true,
  imports: [ReactiveFormsModule, MapComponent, ImageUploadComponent],
  templateUrl: './tour-form.component.html',
  styleUrls: ['./tour-form.component.css']
})
export class TourFormComponent implements OnInit {
  tour = input<Tour | null>(null);
  saveTour = output<TourFormValue>();
  cancelEdit = output<void>();

  submitted = false;
  routePreviewGeoJson: string | null = null;
  isLoadingRoute = false;
  routeError: string | null = null;

  readonly transportTypes: TransportType[] = ['Bike', 'Hike', 'Running', 'Vacation'];
  private readonly durationPattern = /^([0-9]{1,2}):[0-5][0-9](:[0-5][0-9])?$/;
  private readonly fb = inject(FormBuilder);
  private readonly routeService = inject(RouteService);
  private readonly destroyRef = inject(DestroyRef);

  tourForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.maxLength(500)]],
    from: ['', [Validators.required, Validators.maxLength(100)]],
    to: ['', [Validators.required, Validators.maxLength(100)]],
    transportType: ['Bike', Validators.required],
    distance: [1, [Validators.required, Validators.min(0.1)]],
    estimatedTime: ['01:00:00', [Validators.required, Validators.pattern(this.durationPattern)]],
    routeInformation: [''],
    imageFileName: [null as string | null]
  });

  onImageUploaded(fileName: string | null): void {
    this.tourForm.patchValue({ imageFileName: fileName });
  }

  constructor() {
    effect(() => {
      this.tour();
      this.resetForm();
    });
  }

  get isEditing(): boolean {
    return this.tour() !== null;
  }

  ngOnInit(): void {
    const routeFields$ = merge(
      this.tourForm.get('from')!.valueChanges,
      this.tourForm.get('to')!.valueChanges,
      this.tourForm.get('transportType')!.valueChanges
    ).pipe(
      map(() => ({
        from: this.tourForm.get('from')!.value ?? '',
        to: this.tourForm.get('to')!.value ?? '',
        transportType: this.tourForm.get('transportType')!.value ?? '',
      }))
    );

    routeFields$
      .pipe(
        debounceTime(800),
        filter(({ from, to, transportType }) =>
          !!from.trim() && !!to.trim() && !!transportType
        ),
        distinctUntilChanged(
          (a, b) =>
            a.from === b.from &&
            a.to === b.to &&
            a.transportType === b.transportType
        ),
        switchMap(({ from, to, transportType }) => {
          this.isLoadingRoute = true;
          this.routeError = null;
          return this.routeService.getRoute(from, to, transportType).pipe(
            catchError(() => {
              this.isLoadingRoute = false;
              this.routeError = 'Could not calculate route. Check your locations and try again.';
              return EMPTY;
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(result => {
        this.isLoadingRoute = false;
        this.routeError = null;
        this.tourForm.patchValue(
          {
            distance: result.distanceKm,
            estimatedTime: this.minutesToTimespan(result.estimatedMinutes),
            routeInformation: result.routeGeoJson,
          },
          { emitEvent: false }
        );
        this.routePreviewGeoJson = result.routeGeoJson;
      });
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
      this.routePreviewGeoJson = tour.routeInformation ?? null;
      this.routeError = null;
      this.tourForm.reset({
        name: tour.name,
        description: tour.description,
        from: tour.from,
        to: tour.to,
        transportType: tour.transportType,
        distance: tour.distance,
        estimatedTime: tour.estimatedTime,
        routeInformation: tour.routeInformation,
        imageFileName: tour.imageFileName ?? null
      });
      return;
    }

    this.routePreviewGeoJson = null;
    this.routeError = null;
    this.tourForm.reset({
      name: '',
      description: '',
      from: '',
      to: '',
      transportType: 'Bike',
      distance: 1,
      estimatedTime: '01:00:00',
      routeInformation: '',
      imageFileName: null
    });
  }

  private minutesToTimespan(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00`;
  }

  private ensureSeconds(duration: string): string {
    return duration.split(':').length === 2 ? `${duration}:00` : duration;
  }
}
