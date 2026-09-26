using ClientEngagementFlow.Domain.Enums;

namespace ClientEngagementFlow.Domain.Entities
{
    public class ProcessingJob
    {
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public ProcessingStatus Status { get; private set; }
        public DateTime CreatedUtc { get; private set; }
        public DateTime? StartedUtc { get; private set; }
        public DateTime? CompletedUtc { get; private set; }
        public string? FailureReason { get; private set; }

        private ProcessingJob() // empty for ef 
        {
        }

        public ProcessingJob(Guid documentId)
        {
            Id = Guid.NewGuid();
            DocumentId = documentId;
            Status = ProcessingStatus.Queued;
            CreatedUtc = DateTime.UtcNow;
        }

        public void StartProcessing()
        {
            // Can only start processing if queued..
            if (Status != ProcessingStatus.Queued) throw new InvalidOperationException($"Cannot start processing from status '{Status}'.");

            Status = ProcessingStatus.Processing;
            StartedUtc = DateTime.UtcNow;
        }

        public void StartValidation()
        {
            // Can only start validation if currently processing
            if (Status != ProcessingStatus.Processing) throw new InvalidOperationException($"Cannot start processing from status '{Status}'.");

            Status = ProcessingStatus.Validating;
        }

        public void Complete()
        {
            // Can only complete if validted 
            if (Status != ProcessingStatus.Validating) throw new InvalidOperationException($"Cannot start processing from status '{Status}'.");

            Status = ProcessingStatus.Completed;
            CompletedUtc = DateTime.UtcNow;
        }

        public void Fail(string reason)
        {
            if (Status == ProcessingStatus.Completed)
            {
                throw new InvalidOperationException("A completed job cannot be failed.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A failure reason is required.", nameof(reason));
            }


            Status = ProcessingStatus.Failed;
            FailureReason = reason;
            CompletedUtc = DateTime.UtcNow;
        }
    }
}
