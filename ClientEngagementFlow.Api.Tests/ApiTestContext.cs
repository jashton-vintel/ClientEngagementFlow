using Microsoft.AspNetCore.Mvc.Testing;

namespace ClientEngagementFlow.Api.Tests
{
    public sealed class ApiTestContext : IAsyncDisposable
    {
        public WebApplicationFactory<Program> Factory { get; }
        public HttpClient Client { get; }

        public ApiTestContext()
        {
            Factory = new WebApplicationFactory<Program>();
            Client = Factory.CreateClient();
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
        }
    }
}
