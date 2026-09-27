namespace ClientEngagementFlow.Infrastructure.Messaging
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public int MaxDeliveryCount { get; set; } = 10;
    }
}
