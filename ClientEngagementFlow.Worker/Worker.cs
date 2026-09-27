using Azure.Messaging.ServiceBus;
using ClientEngagementFlow.Application.Abstractions.Messaging;
using ClientEngagementFlow.Application.Abstractions.Persistence;
using System.Text.Json;

namespace ClientEngagementFlow.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        private ServiceBusProcessor? _processor;

        public Worker(ServiceBusClient serviceBusClient, IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<Worker> logger)
        {
            _serviceBusClient = serviceBusClient;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var queueName = _configuration["ServiceBus:QueueName"] ?? throw new InvalidOperationException("ServiceBus queue name is not configured.");

            _processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
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

            try
            {

                _logger.LogInformation("Received message {MessageId}. DeliveryCount: {DeliveryCount}", args.Message.MessageId, args.Message.DeliveryCount);

                var message = JsonSerializer.Deserialize<ProcessingJobMessage>(args.Message.Body);

                if (message is null)
                {
                    await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Message body could not be deserialized.");

                    return;
                }

                // safely get a fresh scoped DbContext per message instead of keeping one alive for the whole worker lifetime
                using var scope = _scopeFactory.CreateScope();

                var store = scope.ServiceProvider.GetRequiredService<IProcessingJobStore>();

                var job = await store.GetByIdAsync(message.JobId, args.CancellationToken);

                if (job is null)
                {
                    await args.DeadLetterMessageAsync(args.Message, "JobNotFound", $"Processing job {message.JobId} was not found.");

                    return;
                }

                job.StartProcessing();

                throw new InvalidOperationException("Test processing failure");

                job.StartValidation();
                job.Complete();

                await store.UpdateAsync(job, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed for message {MessageId}. DeliveryCount: {DeliveryCount}", args.Message.MessageId, args.Message.DeliveryCount);

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
