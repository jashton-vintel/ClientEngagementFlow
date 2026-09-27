using Azure.Messaging.ServiceBus;
using ClientEngagementFlow.Application.Abstractions.Messaging;
using ClientEngagementFlow.Application.Abstractions.Persistence;
using ClientEngagementFlow.Infastructure.Persistence;
using ClientEngagementFlow.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClientEngagementFlow.Infastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString =
               configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException(
                   "Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IProcessingJobStore,EfProcessingJobStore>();

            services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));

            var serviceBusConnectionString = configuration["ServiceBus:ConnectionString"] ?? throw new InvalidOperationException("ServiceBus connection string was not found.");

            services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

            services.AddScoped<IProcessingJobPublisher, ServiceBusProcessingJobPublisher>();

            return services;
        }
    }
}
