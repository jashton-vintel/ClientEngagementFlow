using ClientEngagementFlow.Application.Abstractions.Persistence;
using ClientEngagementFlow.Domain.Entities;
using System.Collections.Concurrent;

namespace ClientEngagementFlow.Api.Services
{
    public sealed class InMemoryProcessingJobStore : IProcessingJobStore
    {
        private readonly ConcurrentDictionary<Guid, ProcessingJob> _jobs = new();

        public Task AddAsync(ProcessingJob job, CancellationToken cancellationToken = default)
        {
            _jobs[job.Id] = job;

            return Task.CompletedTask;
        }

        public Task<ProcessingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _jobs.TryGetValue(id, out var job);

            return Task.FromResult(job);
        }

        public Task<IReadOnlyCollection<ProcessingJob>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<ProcessingJob> jobs = _jobs.Values.ToList();

            return Task.FromResult(jobs);
        }
    }
}
