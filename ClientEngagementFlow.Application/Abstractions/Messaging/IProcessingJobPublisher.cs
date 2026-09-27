namespace ClientEngagementFlow.Application.Abstractions.Messaging
{
    public interface IProcessingJobPublisher
    {
        Task PublishAsync(Guid jobId, Guid documentId, CancellationToken cancellationToken = default);
    }
}
