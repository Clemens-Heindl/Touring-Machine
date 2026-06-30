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
  selectedFileName: string | null = null;
  selectedFileSize: string | null = null;

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

    // Store file information for display
    this.selectedFileName = file.name;
    this.selectedFileSize = this.formatFileSize(file.size);

    const reader = new FileReader();

    reader.onload = () => {
      try {
        const tours: Tour[] = JSON.parse(reader.result as string);

        tours.forEach(tour => {
          this.tourState.addTour(tour);
          this.tourService.createOrUpdateTour(tour);
        });
        console.log(`Selected file: ${file.name}, Size: ${this.selectedFileSize}`);
        console.log(`File content: ${JSON.stringify(this.tours())}`);

      } catch (err) {
        console.error('Invalid JSON file', err);
        this.tourState.setApiStatus('Invalid JSON file');
      }
    };

    reader.readAsText(file);
    console.log('Importing tours and logs from a file...');
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }
}
