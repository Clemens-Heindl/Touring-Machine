import { Component, inject } from '@angular/core';
import { TourService } from '../../services/tour.service';
import { TourStateService } from '../../services/tour-state.service';
import { Tour } from '../../models/tour.model';


@Component({
  selector: 'app-import-export',
  standalone: true,
  imports: [],
  templateUrl: './import-export.html',
  styleUrls: ['./import-export.css']
})
export class ImportExportComponent {
  private tourService = inject(TourService);
  tourState = inject(TourStateService);

  readonly tours = this.tourState.tours$;
  readonly selectedTour = this.tourState.selectedTour$;

  showForm = false;
  editingTour: Tour | null = null;

  ngOnInit(): void {
    this.tourState.setApiStatus('Loading tours from the ASP.NET API...');
    this.tourService.getTours().subscribe({
      next: tours => {
        this.tourState.setTours(tours);
      },
      error: () => {
        this.tourState.setApiStatus('API is not reachable. You can still use the local demo data.');
      }
    });
  }

  exportTours() {
    const tours = this.tours();
    const json = JSON.stringify(tours, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = 'tours.json';
    a.click();

    URL.revokeObjectURL(url);
    console.log('Exporting tours and logs as JSON...');
  }     

  importTours(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    const reader = new FileReader();

    reader.onload = () => {
      try {
        const tours: Tour[] = JSON.parse(reader.result as string);

        this.tourState.setTours(tours);

      } catch (err) {
        console.error('Invalid JSON file', err);
        this.tourState.setApiStatus('Invalid JSON file');
      }
    };

    reader.readAsText(file);
    console.log('Importing tours and logs from a file...');
  }
}
