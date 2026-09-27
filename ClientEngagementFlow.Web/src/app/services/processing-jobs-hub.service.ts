import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { MsalService } from '@azure/msal-angular';

@Injectable({
  providedIn: 'root'
})
export class ProcessingJobsHubService {
  private hubConnection?: signalR.HubConnection;

  constructor(
    private readonly msalService: MsalService
  ) {
  }

  startConnection(
    onJobStatusChanged: (jobId: string, status: string) => void
  ): void {

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(
        'https://localhost:7221/hubs/processing-jobs',
        {
          accessTokenFactory: async () => {
            const account = this.msalService.instance.getActiveAccount();

            if (!account) {
              throw new Error('No active MSAL account.');
            }

            const tokenResult =
              await this.msalService.instance.acquireTokenSilent({
                account,
                scopes: [
                  'api://33043fba-9677-43cc-bd5b-c0173f5a1fef/jobs.submit'
                ]
              });

            return tokenResult.accessToken;
          }
        })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on(
      'JobStatusChanged',
      (jobId: string, status: string) => {
        console.log(
          'SignalR job update:',
          jobId,
          status
        );

        onJobStatusChanged(jobId, status);
      }
    );

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connected');
      })
      .catch(error => {
        console.error(
          'SignalR connection failed',
          error
        );
      });
  }
}
