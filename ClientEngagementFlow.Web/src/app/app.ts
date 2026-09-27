import { Component, OnInit, signal } from '@angular/core';
import { ProcessingJob } from './models/processing-job';
import { ProcessingJobsService } from './services/processing-jobs.service';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  jobs = signal<ProcessingJob[]>([]);

  constructor(
    private readonly processingJobsService: ProcessingJobsService,
    private readonly msalService: MsalService
  ) {
  }

  ngOnInit(): void {
    this.processingJobsService.getJobs().subscribe({
      next: jobs => {
        console.log('Jobs returned from API:', jobs);
        console.log('Job count:', jobs.length);

        this.jobs.set(jobs);
      },
      error: error => {
        console.error('Failed to load jobs', error);
      }
    });
  }

  login(): void {
  this.msalService.loginRedirect({
    scopes: [
      'api://33043fba-9677-43cc-bd5b-c0173f5a1fef/jobs.submit'
    ]
  });
  }
}
