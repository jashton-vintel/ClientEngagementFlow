namespace ClientEngagementFlow.Api.Contracts
{
    public record ProcessingJobStatusChangedRequest(Guid JobId, string Status);
}
