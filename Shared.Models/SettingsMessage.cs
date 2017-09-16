namespace Shared.Models
{
    public class SettingsMessage
    {
        public string DoneQueueName { get; set; }

        public string SASToken { get; set; }

        public string StorageUrl { get; set; }

        public string DoneContainerName { get; set; }
    }
}
