using ClientEngagementFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientEngagementFlow.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();

        public DbSet<Document> Documents => Set<Document>();

        public DbSet<Engagement> Engagements => Set<Engagement>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
