import { Component, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
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

  @ViewChild('fileInput', { static: false }) fileInput!: ElementRef<HTMLInputElement>;
  uploadMessage: string = '';
  uploadProgress: number = 0;
  selectedFile: File | null = null;

  constructor(private http: HttpClient) {

  }

  onFileSelected(event: any): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    } else {
      this.selectedFile = null;
    }

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

          // Reset the file input ONLY after successful upload
          if (this.fileInput?.nativeElement) {
            this.fileInput.nativeElement.value = '';
          }

          // Emit the result to parent component
          if (result)
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

