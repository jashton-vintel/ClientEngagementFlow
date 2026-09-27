namespace ClientEngagementFlow.Application.Abstractions.Messaging
{
    public sealed record ProcessingJobMessage(Guid JobId, Guid DocumentId);
}
