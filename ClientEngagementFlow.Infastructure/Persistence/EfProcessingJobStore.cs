using ClientEngagementFlow.Application.Abstractions.Persistence;
using ClientEngagementFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientEngagementFlow.Infastructure.Persistence
{
    public sealed class EfProcessingJobStore : IProcessingJobStore
    {
        private readonly ApplicationDbContext _dbContext;

        public EfProcessingJobStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ProcessingJob job, CancellationToken cancellationToken = default)
        {
            await _dbContext.ProcessingJobs.AddAsync(job,cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<ProcessingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.ProcessingJobs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyCollection<ProcessingJob>> GetAllAsync( CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProcessingJobs.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}
