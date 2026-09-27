import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ProcessingJobsHubService {
  private hubConnection?: signalR.HubConnection;

  startConnection(onJobStatusChanged: (jobId: string, status: string) => void): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7221/hubs/processing-jobs')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on(
      'JobStatusChanged',
      (jobId: string, status: string) => {
        console.log('SignalR job update:', jobId, status);
        onJobStatusChanged(jobId, status);
      }
    );

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connected');
      })
      .catch(error => {
        console.error('SignalR connection failed', error);
      });
  }
}
