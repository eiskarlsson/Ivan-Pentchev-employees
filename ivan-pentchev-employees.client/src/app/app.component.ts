import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  parentUploadResult: any;
  gridResult: any;
  parentErrorMessage: string = '';

  constructor(private http: HttpClient) { }

  ngOnInit() {

  }

  handleUploadComplete(result: any): void {
    console.log('Parent received upload result:', result);
    this.parentUploadResult = result;
    this.parentErrorMessage = ''; // Clear any previous errors

    // You can now use the result in the parent component
    this.processUploadInParent(result);
  }

  handleUploadError(error: any): void {
    // Extract the message from the error object
    const errorMessage = error.message || error.toString();
    console.error('Parent received upload error:', errorMessage);
    this.parentErrorMessage = errorMessage;
    this.parentUploadResult = null;
  }

  private processUploadInParent(result: any): void {
    // Do something with the result in the parent
    console.log('Processing upload in parent:', result);
    this.gridResult = this.getGridData();
    // Examples:
    // - Update other components
    // - Make additional API calls
    // - Update application state
    // - Navigate to another page
  }

  getGridData() {
    return this.parentUploadResult.projectIds.map((projectId: string | number, index: any) => ({
      employee1: this.parentUploadResult.employee1,
      employee2: this.parentUploadResult.employee2,
      projectId: projectId,
      totalDays: this.parentUploadResult.projectOverlapDays[projectId] || 0
    }));
  }


  title = 'ivan-pentchev-employees.client';
}
