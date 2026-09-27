export interface ProcessingJob {
  id: string;
  documentId: string;
  status: string;
  createdUtc: string;
  startedUtc?: string;
  completedUtc?: string;
  failureReason?: string;
}
