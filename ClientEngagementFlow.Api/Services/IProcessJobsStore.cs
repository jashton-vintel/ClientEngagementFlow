using ClientEngagementFlow.Domain.Entities;

namespace ClientEngagementFlow.Api.Services
{
    public interface IProcessJobsStore
    {
        public interface IProcessingJobStore
        {
            Task AddAsync(ProcessingJob job, CancellationToken cancellationToken = default);
            Task<ProcessingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
            Task<IReadOnlyCollection<ProcessingJob>> GetAllAsync(CancellationToken cancellationToken = default);
        }
    }
}
