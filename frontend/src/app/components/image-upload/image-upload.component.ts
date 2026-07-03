import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { HttpEventType } from '@angular/common/http';
import { ImageService } from '../../services/image.service';

/**
 * Reusable drag-and-drop image uploader. Emits the stored file name once the
 * upload completes. Used by the tour form but self-contained and reusable.
 */
@Component({
  selector: 'app-image-upload',
  standalone: true,
  imports: [],
  templateUrl: './image-upload.component.html',
  styleUrls: ['./image-upload.component.css']
})
export class ImageUploadComponent {
  /** Existing stored file name, used to show a preview when editing. */
  @Input() fileName: string | null = null;
  @Output() fileNameChange = new EventEmitter<string | null>();

  private readonly imageService = inject(ImageService);
  private readonly maxBytes = 5 * 1024 * 1024;

  isDragging = false;
  isUploading = false;
  progress = 0;
  error: string | null = null;
  private localPreview: string | null = null;

  get previewUrl(): string {
    return this.localPreview ?? this.imageService.imageUrl(this.fileName);
  }

  get hasImage(): boolean {
    return !!this.localPreview || !!this.fileName;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.handleFile(file);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.handleFile(file);
    }
    input.value = '';
  }

  clear(): void {
    this.localPreview = null;
    this.error = null;
    this.progress = 0;
    this.fileName = null;
    this.fileNameChange.emit(null);
  }

  private handleFile(file: File): void {
    this.error = null;

    if (!file.type.startsWith('image/')) {
      this.error = 'Please choose an image file.';
      return;
    }
    if (file.size > this.maxBytes) {
      this.error = 'Image must be 5 MB or smaller.';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => (this.localPreview = reader.result as string);
    reader.readAsDataURL(file);

    this.isUploading = true;
    this.progress = 0;

    this.imageService.upload(file).subscribe({
      next: event => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.progress = Math.round((100 * event.loaded) / event.total);
        } else if (event.type === HttpEventType.Response) {
          this.isUploading = false;
          this.progress = 100;
          const uploaded = event.body?.fileName ?? null;
          this.fileName = uploaded;
          this.fileNameChange.emit(uploaded);
        }
      },
      error: () => {
        this.isUploading = false;
        this.localPreview = null;
        this.error = 'Upload failed. Please try again.';
      }
    });
  }
}
