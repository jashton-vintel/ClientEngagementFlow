namespace ClientEngagementFlow.Domain.Entities
{
    public class Engagement
    {
        public Guid Id { get; private set; }
        public string ClientName { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedUtc { get; private set; }

        private Engagement()
        {
            ClientName = string.Empty;
            Name = string.Empty;
        }

        public Engagement(string clientName,string name)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException("Client name is required.",nameof(clientName));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Engagement name is required.", nameof(name));
            }

            Id = Guid.NewGuid();
            ClientName = clientName;
            Name = name;
            CreatedUtc = DateTime.UtcNow;
        }
    }
}
