using ClientEngagementFlow.Api.Contracts;
using ClientEngagementFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClientEngagementFlow.Api.Tests
{
    public class ProcessingJobsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        [Fact]
        public async Task Post_job_returns_202_accepted()
        {
            await using var api = new ApiTestContext();

            CreateProcessingJobRequest request = new()
            {
                DocumentId = Guid.NewGuid()
            };

            var response = await api.Client.PostAsJsonAsync("/api/jobs", request);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task Post_job_returns_queued_job()
        {
            await using var api = new ApiTestContext();

            var documentId = Guid.NewGuid();

            CreateProcessingJobRequest request = new()
            {
                DocumentId = documentId
            };

            var response = await api.Client.PostAsJsonAsync("/api/jobs", request);
            var job = await response.Content.ReadFromJsonAsync<ProcessingJobResponse>(JsonOptions);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.NotNull(job);
            Assert.Equal(documentId, job.DocumentId);
            Assert.Equal(ProcessingStatus.Queued, job.Status);
        }

        [Fact]
        public async Task Post_job_returns_location_header()
        {
            await using var api = new ApiTestContext();

            var request = new CreateProcessingJobRequest
            {
                DocumentId = Guid.NewGuid()
            };

            var response = await api.Client.PostAsJsonAsync("/api/jobs", request);

            var job = await response.Content.ReadFromJsonAsync<ProcessingJobResponse>(JsonOptions);

            Assert.NotNull(job);

            Assert.NotNull(response.Headers.Location);

            Assert.Contains(job.Id.ToString(), response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Get_job_returns_created_job()
        {
            await using var api = new ApiTestContext();

            var documentId = Guid.NewGuid();

            CreateProcessingJobRequest createRequest = new()
            {
                DocumentId = documentId
            };

            var createResponse = await api.Client.PostAsJsonAsync("/api/jobs", createRequest);

            var createdJob = await createResponse.Content.ReadFromJsonAsync<ProcessingJobResponse>(JsonOptions);

            Assert.NotNull(createdJob);

            var response = await api.Client.GetAsync($"/api/jobs/{createdJob.Id}");

            var returnedJob = await response.Content.ReadFromJsonAsync<ProcessingJobResponse>(JsonOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(returnedJob);
            Assert.Equal(createdJob.Id, returnedJob.Id);
            Assert.Equal(documentId, returnedJob.DocumentId);
        }

        [Fact]
        public async Task Get_unknown_job_returns_404()
        {
            await using var api = new ApiTestContext();

            var randomId = Guid.NewGuid();

            var response = await api.Client.GetAsync($"/api/jobs/{randomId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_all_jobs_returns_created_jobs()
        {
            await using var api = new ApiTestContext();

            await api.Client.PostAsJsonAsync("/api/jobs", new CreateProcessingJobRequest
            {
                DocumentId = Guid.NewGuid()
            });

            await api.Client.PostAsJsonAsync("/api/jobs", new CreateProcessingJobRequest
            {
                DocumentId = Guid.NewGuid()
            });

            var jobs = await api.Client.GetFromJsonAsync<List<ProcessingJobResponse>>("/api/jobs", JsonOptions);

            Assert.NotNull(jobs);

            Assert.True(jobs.Count == 2);
        }

        [Fact]
        public async Task Post_job_with_empty_document_id_returns_400()
        {
            await using var api = new ApiTestContext();

            CreateProcessingJobRequest request = new()
            {
                DocumentId = Guid.Empty
            };

            var response = await api.Client.PostAsJsonAsync("/api/jobs", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_all_jobs_returns_empty_collection_when_no_jobs_exist()
        {
            await using var api = new ApiTestContext();

            var jobs = await api.Client.GetFromJsonAsync<List<ProcessingJobResponse>>("/api/jobs", JsonOptions);

            Assert.NotNull(jobs);

            Assert.Empty(jobs);
        }

        [Fact]
        public async Task Get_jobs_without_authentication_returns_401()
        {
            await using var api = new ApiTestContext();

            api.Client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");

            var response = await api.Client.GetAsync("/api/jobs");

            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
        }

        [Fact]
        public async Task Post_job_without_authentication_returns_401()
        {
            await using var api = new ApiTestContext();

            api.Client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");

            var response = await api.Client.PostAsJsonAsync("/api/jobs",
                    new CreateProcessingJobRequest
                    {
                        DocumentId = Guid.NewGuid()
                    }, 
                    JsonOptions);

            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
        }

        [Fact]
        public async Task Post_job_without_required_scope_returns_403()
        {
            await using var api = new ApiTestContext();

            api.Client.DefaultRequestHeaders.Add("X-Test-Scopes", "none");

            var response = await api.Client.PostAsJsonAsync("/api/jobs",
                    new CreateProcessingJobRequest
                    {
                        DocumentId = Guid.NewGuid()
                    }, 
                    JsonOptions);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Health_endpoint_allows_anonymous_access()
        {
            await using var api = new ApiTestContext();

            api.Client.DefaultRequestHeaders.Add("X-Test-Anonymous","true");

            var response = await api.Client.GetAsync("/api/jobs/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
