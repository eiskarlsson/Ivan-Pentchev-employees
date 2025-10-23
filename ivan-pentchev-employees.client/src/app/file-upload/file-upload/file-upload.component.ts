import { Component, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  standalone: false,
  styleUrl: './file-upload.component.css'
})

export class FileUploadComponent {
  @Output() uploadComplete = new EventEmitter<any>();
  @Output() uploadError = new EventEmitter<string>();
  uploadMessage: string = '';
  uploadProgress: number = 0;
  selectedFile: File | null = null;

  constructor(private http: HttpClient) {

  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    this.uploadProgress = 0;
    this.uploadMessage = '';
  }

  onUpload(): void {
    if (!this.selectedFile) {
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile, this.selectedFile.name);

    this.http.post('https://localhost:7048/api/fileupload/upload', formData, {
    //this.http.post('https://ivan-pentchev-employeesserver-ap.blackfield-302f15cf.germanywestcentral.azurecontainerapps.io/api/fileupload/upload', formData, {
      reportProgress: true,
      observe: 'events'
    }).subscribe({
      next: (event: any) => {
        if (event.type === 1) {
          if (event.total) {
            this.uploadProgress = Math.round(100 * event.loaded / event.total);
          }
        } else if (event.type === 4) {
          const result = event.body;
          this.uploadMessage = 'File uploaded successfully!';
          this.uploadProgress = 0;
          this.selectedFile = null;

          // Emit the result to parent component
          this.uploadComplete.emit(result);
        }
      },
      error: (error) => {
        this.uploadMessage = 'Upload failed: ' + error.message;
        this.uploadProgress = 0;

        // Convert error to string before emitting
        const errorMessage = error.message || 'Unknown upload error';
        this.uploadError.emit(errorMessage);
      }
    });
  }
}

