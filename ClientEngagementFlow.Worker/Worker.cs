using Azure.Messaging.ServiceBus;
using ClientEngagementFlow.Application.Abstractions.Messaging;
using ClientEngagementFlow.Application.Abstractions.Persistence;
using ClientEngagementFlow.Domain.Enums;
using ClientEngagementFlow.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientEngagementFlow.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor? _processor;

        public Worker(ServiceBusClient serviceBusClient, IServiceScopeFactory scopeFactory, ILogger<Worker> logger, IOptions<ServiceBusOptions> options, 
            IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _serviceBusClient = serviceBusClient;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _serviceBusOptions = options.Value;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _processor = _serviceBusClient.CreateProcessor(_serviceBusOptions.QueueName, new ServiceBusProcessorOptions
            {
                // Deliberate so we get explicit settlements i.e. at least once behaviour instead of deleting a message because it was recieved
                AutoCompleteMessages = false 
            });

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(ct);

            await Task.Delay(Timeout.Infinite, ct);
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            _logger.LogInformation("Received message {MessageId}. DeliveryCount: {DeliveryCount}", args.Message.MessageId, args.Message.DeliveryCount);

            ProcessingJobMessage? message;

            try
            {
                message = JsonSerializer.Deserialize<ProcessingJobMessage>(args.Message.Body);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid message {MessageId}", args.Message.MessageId);

                await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Message body is not valid JSON.");

                return;
            }

            if (message is null)
            {
                await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Message body could not be deserialized.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IProcessingJobStore>();
            var job = await store.GetByIdAsync(message.JobId, args.CancellationToken);

            if (job is null)
            {
                await args.DeadLetterMessageAsync(args.Message, "JobNotFound", $"Processing job {message.JobId} was not found.");
                return;
            }

            // Simple idempotency guard for duplicate service bus delivery
            if (job.Status == ProcessingStatus.Completed)
            {
                _logger.LogInformation("Job {JobId} is already completed. Completing duplicate message.", job.Id);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);

                return;
            }

            try
            {
                job.StartProcessing();

                await store.UpdateAsync(job, args.CancellationToken);
                await NotifyStatusChangedSafelyAsync(job.Id, job.Status.ToString(), args.CancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(5), args.CancellationToken);

                job.StartValidation();

                await store.UpdateAsync(job, args.CancellationToken);
                await NotifyStatusChangedSafelyAsync(job.Id, job.Status.ToString(), args.CancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(5), args.CancellationToken);

                job.Complete();

                await store.UpdateAsync(job, args.CancellationToken);

                await NotifyStatusChangedSafelyAsync(job.Id, job.Status.ToString(),args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed for job {JobId}. DeliveryCount: {DeliveryCount}", job.Id, args.Message.DeliveryCount);

                // Only mark Failed on the final delivery attempt
                if (args.Message.DeliveryCount >= _serviceBusOptions.MaxDeliveryCount)
                {
                    job.Fail(ex.Message);

                    await store.UpdateAsync(job, args.CancellationToken);
                    await NotifyStatusChangedSafelyAsync(job.Id, job.Status.ToString(), args.CancellationToken);
                    await args.DeadLetterMessageAsync(args.Message, "ProcessingFailed", ex.Message, args.CancellationToken);
                    return;
                }

                throw;
            }

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            _logger.LogInformation("Completed processing job {JobId}", job.Id);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Service Bus error. Entity: {EntityPath}, Source: {ErrorSource}", args.EntityPath, args.ErrorSource);

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor is not null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task NotifyStatusChangedAsync(Guid jobId, string status, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();

            var apiKey = _configuration["InternalApi:NotificationApiKey"];

            client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

            var response = await client.PostAsJsonAsync(
                "https://localhost:7221/api/job-notifications",
                new
                {
                    JobId = jobId,
                    Status = status
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        private async Task NotifyStatusChangedSafelyAsync(Guid jobId, string status, CancellationToken cancellationToken)
        {
            try
            {
                await NotifyStatusChangedAsync(jobId, status, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Job {JobId} changed to {Status}, but the real-time notification failed.", jobId, status);
            }
        }
    }
}
