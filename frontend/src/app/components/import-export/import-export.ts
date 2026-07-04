import { Component, inject, OnInit } from '@angular/core';
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
export class ImportExportComponent implements OnInit {
  private tourService = inject(TourService);
  tourState = inject(TourStateService);

  readonly tours = this.tourState.tours$;

  selectedFileName: string | null = null;
  selectedFileSize: string | null = null;
  isBusy = false;

  ngOnInit(): void {
    this.tourState.setApiStatus('Loading tours from the ASP.NET API...');
    this.tourService.getTours().subscribe({
      next: tours => this.tourState.setTours(tours),
      error: () => this.tourState.setApiStatus('API is not reachable. You can still use the local demo data.')
    });
  }

  exportTours(): void {
    this.isBusy = true;
    this.tourService.exportToursFile().subscribe({
      next: blob => {
        this.downloadBlob(blob, 'tours.json');
        this.isBusy = false;
        this.tourState.setApiStatus('Tours exported.');
      },
      error: () => {
        this.isBusy = false;
        this.tourState.setApiStatus('Export failed. Please try again.');
      }
    });
  }

  importTours(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];
    this.selectedFileName = file.name;
    this.selectedFileSize = this.formatFileSize(file.size);

    const reader = new FileReader();
    reader.onload = () => {
      let parsed: Partial<Tour>[];
      try {
        const content = JSON.parse(reader.result as string);
        parsed = Array.isArray(content) ? content : [content];
      } catch {
        this.tourState.setApiStatus('Invalid JSON file.');
        return;
      }

      this.isBusy = true;
      this.tourService.importTours(parsed).subscribe({
        next: () => {
          // Reload from the API so the persisted, server-computed tours are shown.
          this.tourService.getTours().subscribe(tours => this.tourState.setTours(tours));
          this.isBusy = false;
          this.tourState.setApiStatus(`Imported ${parsed.length} tour(s).`);
        },
        error: () => {
          this.isBusy = false;
          this.tourState.setApiStatus('Import failed. Please try again.');
        }
      });
    };

    reader.readAsText(file);
    input.value = '';
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }
}
