import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface FileResponse {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeInBytes: number;
  checksum: string;
  tags?: string;
  createdBy: string;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  uploadFile(file: File, tags?: string): Observable<HttpEvent<FileResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    if (tags) {
      formData.append('tags', tags);
    }

    return this.http.post<FileResponse>(`${this.apiUrl}/api/files`, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  getFiles(pageNumber: number = 1, pageSize: number = 20, searchTerm?: string): Observable<PagedResult<FileResponse>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedResult<FileResponse>>(`${this.apiUrl}/api/files`, { params });
  }

  getFileById(id: string): Observable<FileResponse> {
    return this.http.get<FileResponse>(`${this.apiUrl}/api/files/${id}`);
  }

  downloadFile(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/api/files/${id}/download`, {
      responseType: 'blob'
    });
  }

  previewFile(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/api/files/${id}/preview`, {
      responseType: 'blob'
    });
  }

  softDeleteFile(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/files/${id}`);
  }

  hardDeleteFile(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/files/${id}/hard`);
  }
}

