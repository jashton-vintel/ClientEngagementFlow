import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProcessingJob } from '../models/processing-job';

@Injectable({
  providedIn: 'root'
})
export class ProcessingJobsService {
  private readonly apiUrl = 'https://localhost:7221/api/jobs';

  constructor(private readonly http: HttpClient) {
  }

  getJobs(): Observable<ProcessingJob[]> {
    return this.http.get<ProcessingJob[]>(this.apiUrl);
  }
}
