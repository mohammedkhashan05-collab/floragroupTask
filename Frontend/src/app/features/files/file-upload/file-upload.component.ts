import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService } from '../../../core/services/file.service';
import { ToastService } from '../../../shared/services/toast.service';
import { HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Output() fileUploaded = new EventEmitter<void>();
  
  selectedFiles: File[] = [];
  isDragging = false;
  uploadProgress = 0;
  uploading = false;

  constructor(
    private fileService: FileService,
    private toastService: ToastService
  ) {}

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.addFiles(Array.from(files));
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    if (input?.files && input.files.length > 0) {
      this.addFiles(Array.from(input.files));
    }
  }

  addFiles(files: File[]): void {
    const validFiles = files.filter(file => {
      if (file.size > 100 * 1024 * 1024) {
        this.toastService.warning(`File "${file.name}" exceeds 100MB limit`);
        return false;
      }
      return true;
    });

    this.selectedFiles = [...this.selectedFiles, ...validFiles];
  }

  removeFile(file: File): void {
    this.selectedFiles = this.selectedFiles.filter(f => f !== file);
  }

  uploadFiles(): void {
    if (this.selectedFiles.length === 0 || this.uploading) {
      return;
    }

    this.uploading = true;
    this.uploadProgress = 0;

    const uploadPromises = this.selectedFiles.map(file => {
      return new Promise<void>((resolve, reject) => {
        this.fileService.uploadFile(file).subscribe({
          next: (event) => {
            if (event.type === HttpEventType.UploadProgress && event.total) {
              const progress = Math.round((100 * event.loaded) / event.total);
              this.uploadProgress = progress;
            } else if (event.type === HttpEventType.Response) {
              resolve();
            }
          },
          error: (err) => {
            this.toastService.error(`Failed to upload "${file.name}"`);
            reject(err);
          }
        });
      });
    });

    Promise.all(uploadPromises)
      .then(() => {
        this.toastService.success(`Successfully uploaded ${this.selectedFiles.length} file(s)`);
        this.selectedFiles = [];
        this.uploadProgress = 0;
        this.uploading = false;
        this.fileUploaded.emit();
      })
      .catch(() => {
        this.uploadProgress = 0;
        this.uploading = false;
      });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}

