using Azure.Messaging.ServiceBus;
using ClientEngagementFlow.Application.Abstractions.Messaging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClientEngagementFlow.Infrastructure.Messaging
{
    public class ServiceBusProcessingJobPublisher : IProcessingJobPublisher
    {
        private readonly ServiceBusSender _sender;

        public ServiceBusProcessingJobPublisher(ServiceBusClient client, IOptions<ServiceBusOptions> options)
        {
            _sender = client.CreateSender(options.Value.QueueName);
        }

        public async Task PublishAsync(Guid jobId, Guid documentId, CancellationToken cancellationToken = default)
        {
            var payload = new ProcessingJobMessage(jobId, documentId);

            var body = JsonSerializer.Serialize(payload);

            var message = new ServiceBusMessage(body)
            {
                ContentType = "application/json",
                MessageId = jobId.ToString()
            };

            await _sender.SendMessageAsync(message,cancellationToken);
        }
    }
}
