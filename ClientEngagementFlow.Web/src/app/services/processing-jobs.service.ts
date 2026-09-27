import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProcessingJob } from '../models/processing-job';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProcessingJobsService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/jobs`;

  constructor(private readonly http: HttpClient) {
  }

  getJobs(): Observable<ProcessingJob[]> {
    return this.http.get<ProcessingJob[]>(this.apiUrl);
  }

  createJob(documentId: string): Observable<ProcessingJob> {
    return this.http.post<ProcessingJob>(
      this.apiUrl,
      {
        documentId
      }
    );
  }

}
