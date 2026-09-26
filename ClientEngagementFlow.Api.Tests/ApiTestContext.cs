using ClientEngagementFlow.Infastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClientEngagementFlow.Api.Tests
{
    public sealed class ApiTestContext : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WebApplicationFactory<Program> _factory;

        public HttpClient Client { get; }

        public ApiTestContext()
        {
            _connection = new SqliteConnection("DataSource=:memory:");

            _connection.Open();

            _factory =
                new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.ConfigureServices(services =>
                        {
                            // Removes all instances of SQL since we use that in the api and just use sqlite for testing
                            services.RemoveAll<ApplicationDbContext>();

                            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

                            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

                            services.AddDbContext<ApplicationDbContext>(
                                options =>
                                {
                                    options.UseSqlite(_connection);
                                });
                        });
                    });

            Client = _factory.CreateClient();

            CreateDatabase();
        }

        private void CreateDatabase()
        {
            using var scope = _factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // just for speed..
            dbContext.Database.EnsureCreated();
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();

            await _factory.DisposeAsync();

            await _connection.DisposeAsync();
        }
    }
}
