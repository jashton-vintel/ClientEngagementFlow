using ClientEngagementFlow.Domain.Entities;
using ClientEngagementFlow.Domain.Enums;

namespace ClientEngagementFlow.Domain.Tests
{
    public class ProcessingJobTests
    {
        [Fact]
        public void New_job_should_be_queued()
        {
            var documentId = Guid.NewGuid();

            var job = new ProcessingJob(documentId);

            Assert.Equal(ProcessingStatus.Queued, job.Status);
            Assert.Equal(documentId, job.DocumentId);
        }

        [Fact]
        public void Job_can_progress_through_valid_lifecycle()
        {
            var job = new ProcessingJob(Guid.NewGuid());

            job.StartProcessing();

            Assert.Equal(ProcessingStatus.Processing, job.Status);

            job.StartValidation();

            Assert.Equal(ProcessingStatus.Validating, job.Status);

            job.Complete();

            Assert.Equal(ProcessingStatus.Completed, job.Status);

            Assert.NotNull(job.CompletedUtc);
        }

        [Fact]
        public void Queued_job_cannot_complete_directly()
        {
            var job = new ProcessingJob(Guid.NewGuid());

            Assert.Throws<InvalidOperationException>(() => job.Complete());
        }

        [Fact]
        public void Completed_job_cannot_fail()
        {
            var job = new ProcessingJob(Guid.NewGuid());

            job.StartProcessing();
            job.StartValidation();
            job.Complete();

            Assert.Throws<InvalidOperationException>(() => job.Fail("error"));
        }

        [Fact]
        public void Cannot_fail_without_reason()
        {
            var job = new ProcessingJob(Guid.NewGuid());

            job.StartProcessing();
            job.StartValidation();

            Assert.Throws<ArgumentException>(() => job.Fail(""));
        }
    }
}
