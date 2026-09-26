namespace ClientEngagementFlow.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; private set; }
        public Guid EngagementId { get; private set; }
        public string FileName { get; private set; }
        public string ContentType { get; private set; }
        public DateTime UploadedUtc { get; private set; }

        private Document()
        {
            FileName = string.Empty;
            ContentType = string.Empty;
        }

        public Document(Guid engagementId, string fileName, string contentType)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required.", nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException("Content type is required.", nameof(contentType));
            }

            Id = Guid.NewGuid();
            EngagementId = engagementId;
            FileName = fileName;
            ContentType = contentType;
            UploadedUtc = DateTime.UtcNow;
        }
    }
}
