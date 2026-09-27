import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { MsalService } from '@azure/msal-angular';
import { environment } from '../../environments/environment';

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
        environment.signalRHubUrl,
        {
          accessTokenFactory: async () => {
            const account = this.msalService.instance.getActiveAccount();

            if (!account) {
              throw new Error('No active MSAL account.');
            }

            const tokenResult =
              await this.msalService.instance.acquireTokenSilent({
                account,
                scopes: [environment.apiScope]
              });

            return tokenResult.accessToken;
          }
        })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on(
      'JobStatusChanged',
      (jobId: string, status: string) => {
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
