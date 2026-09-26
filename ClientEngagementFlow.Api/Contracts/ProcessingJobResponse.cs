using ClientEngagementFlow.Domain.Enums;

namespace ClientEngagementFlow.Api.Contracts
{
    public sealed class ProcessingJobResponse
    {
        public Guid Id { get; init; }
        public Guid DocumentId { get; init; }
        public ProcessingStatus Status { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime? StartedUtc { get; init; }
        public DateTime? CompletedUtc { get; init; }
        public string? FailureReason { get; init; }
    }
}
