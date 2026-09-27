import { Component, OnInit, signal } from '@angular/core';
import { ProcessingJob } from './models/processing-job';
import { ProcessingJobsService } from './services/processing-jobs.service';

import { ProcessingJobsHubService } from './services/processing-jobs-hub.service';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  jobs = signal<ProcessingJob[]>([]);
  documentId = signal('');

  constructor(
    private readonly processingJobsService: ProcessingJobsService,
    private readonly processingJobsHubService: ProcessingJobsHubService,
    private readonly msalService: MsalService
  ) {
  }

  ngOnInit(): void {
    this.msalService.instance.initialize()
      .then(() => {
        this.msalService.handleRedirectObservable().subscribe({
          next: result => {
            if (result?.account) {
              this.msalService.instance.setActiveAccount(result.account);
            }

            if (!this.msalService.instance.getActiveAccount()) {
              const accounts = this.msalService.instance.getAllAccounts();

              if (accounts.length > 0) {
                this.msalService.instance.setActiveAccount(accounts[0]);
              }
            }

            if (this.msalService.instance.getActiveAccount()) {
              this.loadJobs();
              this.processingJobsHubService.startConnection((jobId, status) => {
                this.jobs.update(jobs =>
                  jobs.map(job =>
                    job.id === jobId
                      ? { ...job, status }
                      : job
                  )
                );
              });
            }
          },
          error: error => {
            console.error('MSAL redirect error', error);
          }
        });
      })
      .catch(error => {
        console.error('MSAL initialization error', error);
      });
  }

  private loadJobs(): void {
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

  submitJob(): void {
    const documentId = this.documentId().trim();

    if (!documentId) {
      return;
    }

    this.processingJobsService.createJob(documentId).subscribe({
      next: job => {
        console.log('Created job:', job);

        this.jobs.update(jobs => [job, ...jobs]);
        this.documentId.set('');
      },
      error: error => {
        console.error('Failed to create job', error);
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
