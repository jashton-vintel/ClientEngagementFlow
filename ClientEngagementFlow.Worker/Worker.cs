using Azure.Messaging.ServiceBus;
using ClientEngagementFlow.Application.Abstractions.Messaging;
using ClientEngagementFlow.Application.Abstractions.Persistence;
using ClientEngagementFlow.Domain.Enums;
using ClientEngagementFlow.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClientEngagementFlow.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusOptions _serviceBusOptions;

        private ServiceBusProcessor? _processor;

        public Worker(ServiceBusClient serviceBusClient, IServiceScopeFactory scopeFactory, ILogger<Worker> logger, IOptions<ServiceBusOptions> options)
        {
            _serviceBusClient = serviceBusClient;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _serviceBusOptions = options.Value;
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

            // Simple guard to stop potential duplication if sql save failes for now..
            if (job.Status == ProcessingStatus.Completed)
            {
                _logger.LogInformation("Job {JobId} is already completed. Completing duplicate message.", job.Id);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);

                return;
            }

            try
            {
                job.StartProcessing();
                job.StartValidation();

                job.Complete();

                await store.UpdateAsync(job, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message, args.CancellationToken);

                _logger.LogInformation("Completed processing job {JobId}", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed for job {JobId}. DeliveryCount: {DeliveryCount}", job.Id, args.Message.DeliveryCount);

                // If we exceed the max delivery count mark the job as failed, save it and then move to DLQ
                if (args.Message.DeliveryCount >= _serviceBusOptions.MaxDeliveryCount)
                {
                    job.Fail(ex.Message);

                    await store.UpdateAsync(job, args.CancellationToken);
                    await args.DeadLetterMessageAsync(args.Message, "ProcessingFailed", ex.Message, args.CancellationToken);

                    return;
                }

                throw;
            }
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
    }
}
