import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FileService, FileResponse, PagedResult } from '../../../core/services/file.service';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { FileUploadComponent } from '../file-upload/file-upload.component';

@Component({
  selector: 'app-file-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    FileUploadComponent
  ],
  templateUrl: './file-list.component.html',
  styleUrls: ['./file-list.component.scss']
})
export class FileListComponent implements OnInit {
  files: FileResponse[] = [];
  pagedResult?: PagedResult<FileResponse>;
  searchTerm = '';
  currentPage = 1;
  pageSize = 20;
  previewFileData?: { url: string; type: string };

  constructor(
    private fileService: FileService,
    private toastService: ToastService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadFiles();
  }

  loadFiles(): void {
    this.fileService.getFiles(this.currentPage, this.pageSize, this.searchTerm || undefined)
      .subscribe({
        next: (result) => {
          this.pagedResult = result;
          this.files = result.items;
        },
        error: (err) => {
          this.toastService.error('Failed to load files');
        }
      });
  }

  searchFiles(): void {
    this.currentPage = 1;
    this.loadFiles();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadFiles();
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.loadFiles();
  }

  downloadFile(file: FileResponse): void {
    this.fileService.downloadFile(file.id).subscribe({
      next: (blob) => {
        try {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = file.originalFileName;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          // Delay revoking to ensure download starts
          setTimeout(() => window.URL.revokeObjectURL(url), 100);
          this.toastService.success('File downloaded successfully');
        } catch (error) {
          console.error('Error creating download link:', error);
          this.toastService.error('Failed to create download link');
        }
      },
      error: (err) => {
        console.error('Download error:', err);
        this.toastService.error('Failed to download file: ' + (err.message || 'Unknown error'));
      }
    });
  }

  previewFile(file: FileResponse): void {
    this.fileService.previewFile(file.id).subscribe({
      next: (blob) => {
        try {
          const url = window.URL.createObjectURL(blob);
          this.previewFileData = { url, type: file.contentType };
        } catch (error) {
          console.error('Error creating preview:', error);
          this.toastService.error('Failed to create preview');
        }
      },
      error: (err) => {
        console.error('Preview error:', err);
        this.toastService.error('Failed to preview file: ' + (err.message || 'Unknown error'));
      }
    });
  }

  closePreview(): void {
    if (this.previewFileData) {
      window.URL.revokeObjectURL(this.previewFileData.url);
      this.previewFileData = undefined;
    }
  }

  softDeleteFile(file: FileResponse): void {
    if (confirm(`Are you sure you want to delete "${file.originalFileName}"?`)) {
      this.fileService.softDeleteFile(file.id).subscribe({
        next: () => {
          this.toastService.success('File deleted successfully');
          this.loadFiles();
        },
        error: () => {
          this.toastService.error('Failed to delete file');
        }
      });
    }
  }

  hardDeleteFile(file: FileResponse): void {
    if (confirm(`Are you sure you want to permanently delete "${file.originalFileName}"? This action cannot be undone.`)) {
      this.fileService.hardDeleteFile(file.id).subscribe({
        next: () => {
          this.toastService.success('File permanently deleted');
          this.loadFiles();
        },
        error: () => {
          this.toastService.error('Failed to delete file');
        }
      });
    }
  }

  canPreview(contentType: string): boolean {
    return contentType.startsWith('image/') || contentType === 'application/pdf';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }

  isAdmin(): boolean {
    const user = this.authService.getCurrentUser();
    return user?.role === 'admin';
  }
}

